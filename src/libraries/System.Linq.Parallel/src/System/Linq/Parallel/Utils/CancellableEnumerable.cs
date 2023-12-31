// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// CancellableEnumerable.cs
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Parallel;
using System.Text;
using System.Threading;

namespace System.Linq.Parallel
{
    internal static class CancellableEnumerable
    {
        /// <summary>
        /// Wraps an enumerable with a cancellation checker. The enumerator handed out by the source enumerable
        /// will be wrapped by an object that periodically checks whether a particular cancellation token has
        /// been cancelled. If so, the next call to MoveNext() will throw an OperationCancelledException.
        /// </summary>
        internal static IEnumerable<TElement> Wrap<TElement>(IEnumerable<TElement> source, CancellationToken token)
        {
            int count = 0;
            foreach (TElement element in source)
            {
                if ((count++ & CancellationState.POLL_INTERVAL) == 0)
                    token.ThrowIfCancellationRequested();

                yield return element;
            }
        }
    }
}
