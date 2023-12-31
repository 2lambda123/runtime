// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;
using Internal;

namespace System.Runtime.CompilerServices
{
    /// <summary>Represents a builder for asynchronous methods that returns a <see cref="ValueTask{TResult}"/>.</summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    [StructLayout(LayoutKind.Auto)]
    public struct PoolingAsyncValueTaskMethodBuilder<TResult>
    {
        /// <summary>Sentinel object used to indicate that the builder completed synchronously and successfully.</summary>
        /// <remarks>
        /// To avoid memory safety issues even in the face of invalid race conditions, we ensure that the type of this object
        /// is valid for the mode in which we're operating.  As such, it's cached on the generic builder per TResult
        /// rather than having one sentinel instance for all types.
        /// </remarks>
        internal static readonly StateMachineBox s_syncSuccessSentinel = new SyncSuccessSentinelStateMachineBox();

        /// <summary>The wrapped state machine or task.  If the operation completed synchronously and successfully, this will be a sentinel object compared by reference identity.</summary>
        private StateMachineBox? m_task; // Debugger depends on the exact name of this field.
        /// <summary>The result for this builder if it's completed synchronously, in which case <see cref="m_task"/> will be <see cref="s_syncSuccessSentinel"/>.</summary>
        private TResult _result;

        /// <summary>Creates an instance of the <see cref="PoolingAsyncValueTaskMethodBuilder{TResult}"/> struct.</summary>
        /// <returns>The initialized instance.</returns>
        public static PoolingAsyncValueTaskMethodBuilder<TResult> Create() => default;

        /// <summary>Begins running the builder with the associated state machine.</summary>
        /// <typeparam name="TStateMachine">The type of the state machine.</typeparam>
        /// <param name="stateMachine">The state machine instance, passed by reference.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine =>
            AsyncMethodBuilderCore.Start(ref stateMachine);

        /// <summary>Associates the builder with the specified state machine.</summary>
        /// <param name="stateMachine">The state machine instance to associate with the builder.</param>
        public void SetStateMachine(IAsyncStateMachine stateMachine) =>
            AsyncMethodBuilderCore.SetStateMachine(stateMachine, task: null);

        /// <summary>Marks the value task as successfully completed.</summary>
        /// <param name="result">The result to use to complete the value task.</param>
        public void SetResult(TResult result)
        {
            if (m_task is null)
            {
                _result = result;
                m_task = s_syncSuccessSentinel;
            }
            else
            {
                m_task.SetResult(result);
            }
        }

        /// <summary>Marks the value task as failed and binds the specified exception to the value task.</summary>
        /// <param name="exception">The exception to bind to the value task.</param>
        public void SetException(Exception exception) =>
            SetException(exception, ref m_task);

        internal static void SetException(Exception exception, [NotNull] ref StateMachineBox? boxFieldRef)
        {
            if (exception is null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.exception);
            }

            (boxFieldRef ??= CreateWeaklyTypedStateMachineBox()).SetException(exception);
        }

        /// <summary>Gets the value task for this builder.</summary>
        public ValueTask<TResult> Task
        {
            get
            {
                if (m_task == s_syncSuccessSentinel)
                {
                    return new ValueTask<TResult>(_result);
                }

                // With normal access paterns, m_task should always be non-null here: the async method should have
                // either completed synchronously, in which case SetResult would have set m_task to a non-null object,
                // or it should be completing asynchronously, in which case AwaitUnsafeOnCompleted would have similarly
                // initialized m_task to a state machine object.  However, if the type is used manually (not via
                // compiler-generated code) and accesses Task directly, we force it to be initialized.  Things will then
                // "work" but in a degraded mode, as we don't know the TStateMachine type here, and thus we use a box around
                // the interface instead.

                StateMachineBox? box = m_task ??= CreateWeaklyTypedStateMachineBox();
                return new ValueTask<TResult>(box, box.Version);
            }
        }

        /// <summary>Schedules the state machine to proceed to the next action when the specified awaiter completes.</summary>
        /// <typeparam name="TAwaiter">The type of the awaiter.</typeparam>
        /// <typeparam name="TStateMachine">The type of the state machine.</typeparam>
        /// <param name="awaiter">the awaiter</param>
        /// <param name="stateMachine">The state machine.</param>
        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine =>
            AwaitOnCompleted(ref awaiter, ref stateMachine, ref m_task);

        internal static void AwaitOnCompleted<TAwaiter, TStateMachine>(
            ref TAwaiter awaiter, ref TStateMachine stateMachine, ref StateMachineBox? box)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            try
            {
                awaiter.OnCompleted(GetStateMachineBox(ref stateMachine, ref box).MoveNextAction);
            }
            catch (Exception e)
            {
                Threading.Tasks.Task.ThrowAsync(e, targetContext: null);
            }
        }

        /// <summary>Schedules the state machine to proceed to the next action when the specified awaiter completes.</summary>
        /// <typeparam name="TAwaiter">The type of the awaiter.</typeparam>
        /// <typeparam name="TStateMachine">The type of the state machine.</typeparam>
        /// <param name="awaiter">the awaiter</param>
        /// <param name="stateMachine">The state machine.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine =>
            AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine, ref m_task);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(
            ref TAwaiter awaiter, ref TStateMachine stateMachine, [NotNull] ref StateMachineBox? boxRef)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            IAsyncStateMachineBox box = GetStateMachineBox(ref stateMachine, ref boxRef);
            AsyncTaskMethodBuilder<VoidTaskResult>.AwaitUnsafeOnCompleted(ref awaiter, box);
        }

        /// <summary>Gets the "boxed" state machine object.</summary>
        /// <typeparam name="TStateMachine">Specifies the type of the async state machine.</typeparam>
        /// <param name="stateMachine">The state machine.</param>
        /// <param name="boxFieldRef">A reference to the field containing the initialized state machine box.</param>
        /// <returns>The "boxed" state machine.</returns>
        private static IAsyncStateMachineBox GetStateMachineBox<TStateMachine>(
            ref TStateMachine stateMachine,
            [NotNull] ref StateMachineBox? boxFieldRef)
            where TStateMachine : IAsyncStateMachine
        {
            ExecutionContext? currentContext = ExecutionContext.Capture();

            // Check first for the most common case: not the first yield in an async method.
            // In this case, the first yield will have already "boxed" the state machine in
            // a strongly-typed manner into an AsyncStateMachineBox.  It will already contain
            // the state machine as well as a MoveNextDelegate and a context.  The only thing
            // we might need to do is update the context if that's changed since it was stored.
            if (boxFieldRef is StateMachineBox<TStateMachine> stronglyTypedBox)
            {
                if (stronglyTypedBox.Context != currentContext)
                {
                    stronglyTypedBox.Context = currentContext;
                }

                return stronglyTypedBox;
            }

            // The least common case: we have a weakly-typed boxed.  This results if the debugger
            // or some other use of reflection accesses a property like ObjectIdForDebugger.  In
            // such situations, we need to get an object to represent the builder, but we don't yet
            // know the type of the state machine, and thus can't use TStateMachine.  Instead, we
            // use the IAsyncStateMachine interface, which all TStateMachines implement.  This will
            // result in a boxing allocation when storing the TStateMachine if it's a struct, but
            // this only happens in active debugging scenarios where such performance impact doesn't
            // matter.
            if (boxFieldRef is StateMachineBox<IAsyncStateMachine> weaklyTypedBox)
            {
                // If this is the first await, we won't yet have a state machine, so store it.
                if (weaklyTypedBox.StateMachine is null)
                {
                    Debugger.NotifyOfCrossThreadDependency(); // same explanation as with usage below
                    weaklyTypedBox.StateMachine = stateMachine;
                }

                // Update the context.  This only happens with a debugger, so no need to spend
                // extra IL checking for equality before doing the assignment.
                weaklyTypedBox.Context = currentContext;
                return weaklyTypedBox;
            }

            // Alert a listening debugger that we can't make forward progress unless it slips threads.
            // If we don't do this, and a method that uses "await foo;" is invoked through funceval,
            // we could end up hooking up a callback to push forward the async method's state machine,
            // the debugger would then abort the funceval after it takes too long, and then continuing
            // execution could result in another callback being hooked up.  At that point we have
            // multiple callbacks registered to push the state machine, which could result in bad behavior.
            Debugger.NotifyOfCrossThreadDependency();

            // At this point, m_task should really be null, in which case we want to create the box.
            // However, in a variety of debugger-related (erroneous) situations, it might be non-null,
            // e.g. if the Task property is examined in a Watch window, forcing it to be lazily-initialized
            // as a Task<TResult> rather than as an ValueTaskStateMachineBox.  The worst that happens in such
            // cases is we lose the ability to properly step in the debugger, as the debugger uses that
            // object's identity to track this specific builder/state machine.  As such, we proceed to
            // overwrite whatever's there anyway, even if it's non-null.
            StateMachineBox<TStateMachine> box = StateMachineBox<TStateMachine>.RentFromCache();
            boxFieldRef = box; // important: this must be done before storing stateMachine into box.StateMachine!
            box.StateMachine = stateMachine;
            box.Context = currentContext;

            return box;
        }

        /// <summary>
        /// Creates a box object for use when a non-standard access pattern is employed, e.g. when Task
        /// is evaluated in the debugger prior to the async method yielding for the first time.
        /// </summary>
        internal static StateMachineBox CreateWeaklyTypedStateMachineBox() => new StateMachineBox<IAsyncStateMachine>();

        /// <summary>
        /// Gets an object that may be used to uniquely identify this builder to the debugger.
        /// </summary>
        /// <remarks>
        /// This property lazily instantiates the ID in a non-thread-safe manner.
        /// It must only be used by the debugger and tracing purposes, and only in a single-threaded manner
        /// when no other threads are in the middle of accessing this or other members that lazily initialize the box.
        /// </remarks>
        internal object ObjectIdForDebugger => m_task ??= CreateWeaklyTypedStateMachineBox();

        /// <summary>The base type for all value task box reusable box objects, regardless of state machine type.</summary>
        internal abstract class StateMachineBox : IValueTaskSource<TResult>, IValueTaskSource
        {
            /// <summary>A delegate to the MoveNext method.</summary>
            protected Action? _moveNextAction;
            /// <summary>Captured ExecutionContext with which to invoke MoveNext.</summary>
            public ExecutionContext? Context;
            /// <summary>Implementation for IValueTaskSource interfaces.</summary>
            protected ManualResetValueTaskSourceCore<TResult> _valueTaskSource;

            /// <summary>Completes the box with a result.</summary>
            /// <param name="result">The result.</param>
            public void SetResult(TResult result) =>
                _valueTaskSource.SetResult(result);

            /// <summary>Completes the box with an error.</summary>
            /// <param name="error">The exception.</param>
            public void SetException(Exception error) =>
                _valueTaskSource.SetException(error);

            /// <summary>Gets the status of the box.</summary>
            public ValueTaskSourceStatus GetStatus(short token) => _valueTaskSource.GetStatus(token);

            /// <summary>Schedules the continuation action for this box.</summary>
            public void OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags) =>
                _valueTaskSource.OnCompleted(continuation, state, token, flags);

            /// <summary>Gets the current version number of the box.</summary>
            public short Version => _valueTaskSource.Version;

            /// <summary>Implemented by derived type.</summary>
            TResult IValueTaskSource<TResult>.GetResult(short token) => throw NotImplemented.ByDesign;

            /// <summary>Implemented by derived type.</summary>
            void IValueTaskSource.GetResult(short token) => throw NotImplemented.ByDesign;
        }

        /// <summary>Type used as a singleton to indicate synchronous success for an async method.</summary>
        private sealed class SyncSuccessSentinelStateMachineBox : StateMachineBox
        {
            public SyncSuccessSentinelStateMachineBox() => SetResult(default!);
        }

        /// <summary>Provides a strongly-typed box object based on the specific state machine type in use.</summary>
        private sealed class StateMachineBox<TStateMachine> :
            StateMachineBox,
            IValueTaskSource<TResult>, IValueTaskSource, IAsyncStateMachineBox, IThreadPoolWorkItem
            where TStateMachine : IAsyncStateMachine
        {
            /// <summary>Delegate used to invoke on an ExecutionContext when passed an instance of this box type.</summary>
            private static readonly ContextCallback s_callback = ExecutionContextCallback;
            /// <summary>Per-core cache of boxes, with one box per core.</summary>
            /// <remarks>Each element is padded to expected cache-line size so as to minimize false sharing.</remarks>
            private static readonly PaddedReference[] s_perCoreCache = new PaddedReference[Environment.ProcessorCount];
            /// <summary>Thread-local cache of boxes. This currently only ever stores one.</summary>
            [ThreadStatic]
            private static StateMachineBox<TStateMachine>? t_tlsCache;

            /// <summary>The state machine itself.</summary>
            public TStateMachine? StateMachine;

            /// <summary>Gets a box object to use for an operation.  This may be a reused, pooled object, or it may be new.</summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)] // only one caller
            internal static StateMachineBox<TStateMachine> RentFromCache()
            {
                // First try to get a box from the per-thread cache.
                StateMachineBox<TStateMachine>? box = t_tlsCache;
                if (box is not null)
                {
                    t_tlsCache = null;
                }
                else
                {
                    // If we can't, then try to get a box from the per-core cache.
                    ref StateMachineBox<TStateMachine>? slot = ref PerCoreCacheSlot;
                    if (slot is null ||
                        (box = Interlocked.Exchange<StateMachineBox<TStateMachine>?>(ref slot, null)) is null)
                    {
                        // If we can't, just create a new one.
                        box = new StateMachineBox<TStateMachine>();
                    }
                }

                return box;
            }

            /// <summary>Returns this instance to the cache.</summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)] // only two callers
            private void ReturnToCache()
            {
                // Clear out the state machine and associated context to avoid keeping arbitrary state referenced by
                // lifted locals, and reset the instance for another await.
                ClearStateUponCompletion();
                _valueTaskSource.Reset();

                // If the per-thread cache is empty, store this into it..
                if (t_tlsCache is null)
                {
                    t_tlsCache = this;
                }
                else
                {
                    // Otherwise, store it into the per-core cache.
                    ref StateMachineBox<TStateMachine>? slot = ref PerCoreCacheSlot;
                    if (slot is null)
                    {
                        // Try to avoid the write if we know the slot isn't empty (we may still have a benign race condition and
                        // overwrite what's there if something arrived in the interim).
                        Volatile.Write(ref slot, this);
                    }
                }
            }

            /// <summary>Gets the slot in <see cref="s_perCoreCache"/> for the current core.</summary>
            private static ref StateMachineBox<TStateMachine>? PerCoreCacheSlot
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)] // only two callers are RentFrom/ReturnToCache
                get
                {
                    // Get the current processor ID.  We need to ensure it fits within s_perCoreCache, so we
                    // could % by its length, but we can do so instead by Environment.ProcessorCount, which will be a const
                    // in tier 1, allowing better code gen, and then further use uints for even better code gen.
                    Debug.Assert(s_perCoreCache.Length == Environment.ProcessorCount, $"{s_perCoreCache.Length} != {Environment.ProcessorCount}");
                    int i = (int)((uint)Thread.GetCurrentProcessorId() % (uint)Environment.ProcessorCount);

                    // We want an array of StateMachineBox<> objects, each consuming its own cache line so that
                    // elements don't cause false sharing with each other.  But we can't use StructLayout.Explicit
                    // with generics.  So we use object fields, but always reinterpret them (for all reads and writes
                    // to avoid any safety issues) as StateMachineBox<> instances.
#if DEBUG
                    object? transientValue = s_perCoreCache[i].Object;
                    Debug.Assert(transientValue is null || transientValue is StateMachineBox<TStateMachine>,
                        $"Expected null or {nameof(StateMachineBox<TStateMachine>)}, got '{transientValue}'");
#endif
                    return ref Unsafe.As<object?, StateMachineBox<TStateMachine>?>(ref s_perCoreCache[i].Object);
                }
            }

            /// <summary>
            /// Clear out the state machine and associated context to avoid keeping arbitrary state referenced by lifted locals.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void ClearStateUponCompletion()
            {
                StateMachine = default;
                Context = default;
            }

            /// <summary>
            /// Used to initialize s_callback above. We don't use a lambda for this on purpose: a lambda would
            /// introduce a new generic type behind the scenes that comes with a hefty size penalty in AOT builds.
            /// </summary>
            private static void ExecutionContextCallback(object? s)
            {
                // Only used privately to pass directly to EC.Run
                Debug.Assert(s is StateMachineBox<TStateMachine>, $"Expected {nameof(StateMachineBox<TStateMachine>)}, got '{s}'");
                Unsafe.As<StateMachineBox<TStateMachine>>(s).StateMachine!.MoveNext();
            }

            /// <summary>A delegate to the <see cref="MoveNext()"/> method.</summary>
            public Action MoveNextAction => _moveNextAction ??= new Action(MoveNext);

            /// <summary>Invoked to run MoveNext when this instance is executed from the thread pool.</summary>
            void IThreadPoolWorkItem.Execute() => MoveNext();

            /// <summary>Calls MoveNext on <see cref="StateMachine"/></summary>
            public void MoveNext()
            {
                ExecutionContext? context = Context;

                if (context is null)
                {
                    Debug.Assert(StateMachine is not null, $"Null {nameof(StateMachine)}");
                    StateMachine.MoveNext();
                }
                else
                {
                    ExecutionContext.RunInternal(context, s_callback, this);
                }
            }

            /// <summary>Get the result of the operation.</summary>
            TResult IValueTaskSource<TResult>.GetResult(short token)
            {
                try
                {
                    return _valueTaskSource.GetResult(token);
                }
                finally
                {
                    ReturnToCache();
                }
            }

            /// <summary>Get the result of the operation.</summary>
            void IValueTaskSource.GetResult(short token)
            {
                try
                {
                    _valueTaskSource.GetResult(token);
                }
                finally
                {
                    ReturnToCache();
                }
            }

            /// <summary>Gets the state machine as a boxed object.  This should only be used for debugging purposes.</summary>
            IAsyncStateMachine IAsyncStateMachineBox.GetStateMachineObject() => StateMachine!; // likely boxes, only use for debugging
        }
    }
}
