// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// Specifies whether to enable various legacy or new opt-in behaviors.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = false)]
    public sealed class RuntimeCompatibilityAttribute : Attribute
    {
        public RuntimeCompatibilityAttribute()
        {
            // legacy behavior is the default, and WrapNonExceptionThrows is implicitly
            // false thanks to the CLR's guarantee of zeroed memory.
        }

        // If a non-CLSCompliant exception (i.e. one that doesn't derive from System.Exception) is
        // thrown, should it be wrapped up in a System.Runtime.CompilerServices.RuntimeWrappedException
        // instance when presented to catch handlers?
        public bool WrapNonExceptionThrows { get; set; }
    }
}
