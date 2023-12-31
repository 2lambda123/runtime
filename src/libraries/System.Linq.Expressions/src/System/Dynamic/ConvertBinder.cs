// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Dynamic.Utils;
using System.Linq.Expressions;

namespace System.Dynamic
{
    /// <summary>
    /// Represents the convert dynamic operation at the call site, providing the binding semantic and the details about the operation.
    /// </summary>
    [RequiresDynamicCode(Expression.CallSiteRequiresDynamicCode)]
    public abstract class ConvertBinder : DynamicMetaObjectBinder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConvertBinder" />.
        /// </summary>
        /// <param name="type">The type to convert to.</param>
        /// <param name="explicit">true if the conversion should consider explicit conversions; otherwise, false.</param>
        protected ConvertBinder(Type type, bool @explicit)
        {
            ArgumentNullException.ThrowIfNull(type);

            Type = type;
            Explicit = @explicit;
        }

        /// <summary>
        /// The type to convert to.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Gets the value indicating if the conversion should consider explicit conversions.
        /// </summary>
        public bool Explicit { get; }

        /// <summary>
        /// Performs the binding of the dynamic convert operation if the target dynamic object cannot bind.
        /// </summary>
        /// <param name="target">The target of the dynamic convert operation.</param>
        /// <returns>The <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
        public DynamicMetaObject FallbackConvert(DynamicMetaObject target)
        {
            return FallbackConvert(target, null);
        }

        /// <summary>
        /// When overridden in the derived class, performs the binding of the dynamic convert operation if the target dynamic object cannot bind.
        /// </summary>
        /// <param name="target">The target of the dynamic convert operation.</param>
        /// <param name="errorSuggestion">The binding result to use if binding fails, or null.</param>
        /// <returns>The <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
        public abstract DynamicMetaObject FallbackConvert(DynamicMetaObject target, DynamicMetaObject? errorSuggestion);

        /// <summary>
        /// Performs the binding of the dynamic convert operation.
        /// </summary>
        /// <param name="target">The target of the dynamic convert operation.</param>
        /// <param name="args">An array of arguments of the dynamic convert operation.</param>
        /// <returns>The <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
        public sealed override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[]? args)
        {
            ArgumentNullException.ThrowIfNull(target);
            ContractUtils.Requires(args == null || args.Length == 0, nameof(args));

            return target.BindConvert(this);
        }

        /// <summary>
        /// Always returns <c>true</c> because this is a standard <see cref="DynamicMetaObjectBinder"/>.
        /// </summary>
        internal sealed override bool IsStandardBinder => true;

        /// <summary>
        /// The result type of the operation.
        /// </summary>
        public sealed override Type ReturnType => Type;
    }
}
