// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Runtime.Versioning;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace System.Diagnostics
{
    internal static partial class ProcessManager
    {
        /// <summary>Gets whether the process with the specified ID on the specified machine is currently running.</summary>
        /// <param name="processId">The process ID.</param>
        /// <param name="machineName">The machine name.</param>
        /// <returns>true if the process is running; otherwise, false.</returns>
        public static bool IsProcessRunning(int processId, string machineName)
        {
            ThrowIfRemoteMachine(machineName);
            return IsProcessRunning(processId);
        }

        /// <summary>Gets whether the process with the specified ID is currently running.</summary>
        /// <param name="processId">The process ID.</param>
        /// <returns>true if the process is running; otherwise, false.</returns>
        public static bool IsProcessRunning(int processId)
        {
            // kill with signal==0 means to not actually send a signal.
            // If we get back 0, the process is still alive.
            int output = Interop.Sys.Kill(processId, Interop.Sys.Signals.None);
            // If kill set errno=EPERM, assume querying process is alive.
            return 0 == output || (-1 == output && Interop.Error.EPERM == Interop.Sys.GetLastError());
        }

        /// <summary>Gets the ProcessInfo for the specified process ID on the specified machine.</summary>
        /// <param name="processId">The process ID.</param>
        /// <param name="machineName">The machine name.</param>
        /// <returns>The ProcessInfo for the process if it could be found; otherwise, null.</returns>
        public static ProcessInfo? GetProcessInfo(int processId, string machineName)
        {
            ThrowIfRemoteMachine(machineName);
            return CreateProcessInfo(processId);
        }

        /// <summary>Gets the IDs of all processes on the specified machine.</summary>
        /// <param name="machineName">The machine to examine.</param>
        /// <returns>An array of process IDs from the specified machine.</returns>
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
        [SupportedOSPlatform("maccatalyst")]
        public static int[] GetProcessIds(string machineName)
        {
            ThrowIfRemoteMachine(machineName);
            return GetProcessIds();
        }

        /// <summary>Gets the ID of a process from a handle to the process.</summary>
        /// <param name="processHandle">The handle.</param>
        /// <returns>The process ID.</returns>
        public static int GetProcessIdFromHandle(SafeProcessHandle processHandle)
        {
            return processHandle.ProcessId;
        }

        private static bool IsRemoteMachineCore(string machineName)
        {
            return
                machineName != "." &&
                machineName != Interop.Sys.GetHostName();
        }

        internal static void ThrowIfRemoteMachine(string machineName)
        {
            if (IsRemoteMachine(machineName))
            {
                throw new PlatformNotSupportedException(SR.RemoteMachinesNotSupported);
            }
        }
    }
}
