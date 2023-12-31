// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;

internal static partial class Interop
{
    internal static partial class Kernel32
    {
        [SuppressGCTransition]
        [LibraryImport(Libraries.Kernel32, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool SetThreadErrorMode(
            uint dwNewMode,
            out uint lpOldMode);

        internal const int SEM_FAILCRITICALERRORS = 0x00000001;
        internal const int SEM_NOOPENFILEERRORBOX = 0x00008000;
    }
}
