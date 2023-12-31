// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace Internal.Cryptography.Pal.Windows
{
    //
    // Some CryptoApis take structure parameters that have a huge number of sub blocks attached. This convenience class lets us
    // allocate and preserve a collection of heap blocks in one single object.
    //
    internal sealed class HeapBlockRetainer : IDisposable
    {
        public HeapBlockRetainer()
        {
            _mustLive = new List<object>();
            _blocks = new List<SafeHeapAllocHandle>();
        }

        public IntPtr Alloc(int cbSize)
        {
            if (cbSize < 0)
                throw new OverflowException();
            SafeHeapAllocHandle hBlock = SafeHeapAllocHandle.Alloc(cbSize);
            _blocks.Add(hBlock);
            return hBlock.DangerousGetHandle();
        }

        public IntPtr Alloc(int howMany, int cbElement)
        {
            if (cbElement < 0 || howMany < 0)
                throw new OverflowException();

            int cbSize = checked(howMany * cbElement);
            return Alloc(cbSize);
        }

        public unsafe IntPtr AllocAsciiString(string s)
        {
            int length = Encoding.ASCII.GetByteCount(s);
            length++; // for null termination

            IntPtr pb = Alloc(length);

            var ascii = new Span<byte>((byte*)pb, length);
            Encoding.ASCII.GetBytes(s, ascii);
            ascii[^1] = 0;

            return pb;
        }

        public IntPtr AllocBytes(byte[] data)
        {
            IntPtr pData = Alloc(data.Length);
            Marshal.Copy(data, 0, pData, data.Length);
            return pData;
        }

        public void KeepAlive(object o)
        {
            _mustLive.Add(o);
        }

        public void Dispose()
        {
            if (_blocks != null)
            {
                foreach (SafeHeapAllocHandle h in _blocks)
                {
                    h.Dispose();
                }
            }
            _blocks = null!;
        }

        private readonly List<object> _mustLive;
        private List<SafeHeapAllocHandle> _blocks;
    }
}
