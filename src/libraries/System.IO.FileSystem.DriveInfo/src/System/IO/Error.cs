// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace System.IO
{
    internal static class Error
    {
        // An alternative to Win32Marshal with friendlier messages for drives
        internal static Exception GetExceptionForLastWin32DriveError(string driveName)
        {
            int errorCode = Marshal.GetLastPInvokeError();
            return GetExceptionForWin32DriveError(errorCode, driveName);
        }

        internal static Exception GetExceptionForWin32DriveError(int errorCode, string driveName)
        {
            switch (errorCode)
            {
                case Interop.Errors.ERROR_PATH_NOT_FOUND:
                case Interop.Errors.ERROR_INVALID_DRIVE:
                    return new DriveNotFoundException(SR.Format(SR.IO_DriveNotFound_Drive, driveName));

                default:
                    return Win32Marshal.GetExceptionForWin32Error(errorCode, driveName);
            }
        }
    }
}
