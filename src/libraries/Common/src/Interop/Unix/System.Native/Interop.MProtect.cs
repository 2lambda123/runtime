// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

internal static partial class Interop
{
    internal static partial class Sys
    {
        [LibraryImport(Libraries.SystemNative, EntryPoint = "SystemNative_MProtect", SetLastError = true)]
        internal static partial int MProtect(IntPtr addr, ulong len, MemoryMappedProtections prot);
    }
}
