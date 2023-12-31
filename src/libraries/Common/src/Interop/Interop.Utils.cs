// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers;

internal static partial class Interop
{
    /// <summary>
    /// Helper for making interop calls that return a string, but we don't know
    /// the correct size of buffer to make. So invoke the interop call with an
    /// increasing buffer until the size is big enough.
    /// </summary>
    internal static bool CallStringMethod<TArg1, TArg2, TArg3>(
        SpanFunc<char, TArg1, TArg2, TArg3, Globalization.ResultCode> interopCall,
        TArg1 arg1, TArg2 arg2, TArg3 arg3,
        out string? result)
    {
        const int InitialSize = 256; // arbitrary stack allocation size
        const int MaxHeapSize = 1280; // max from previous version of the code, starting at 80 and doubling four times

        Span<char> buffer = stackalloc char[InitialSize];
        Globalization.ResultCode resultCode = interopCall(buffer, arg1, arg2, arg3);

        if (resultCode == Globalization.ResultCode.Success)
        {
            result = buffer.Slice(0, buffer.IndexOf('\0')).ToString();
            return true;
        }

        if (resultCode == Globalization.ResultCode.InsufficientBuffer)
        {
            // Increase the string size and try again
            buffer = new char[MaxHeapSize];
            if (interopCall(buffer, arg1, arg2, arg3) == Globalization.ResultCode.Success)
            {
                result = buffer.Slice(0, buffer.IndexOf('\0')).ToString();
                return true;
            }
        }

        result = null;
        return false;
    }
}
