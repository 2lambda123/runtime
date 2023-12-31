// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace System.IO
{
    [Serializable]
    [TypeForwardedFrom("mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]
    public class IOException : SystemException
    {
        public IOException()
            : base(SR.Arg_IOException)
        {
            HResult = HResults.COR_E_IO;
        }

        public IOException(string? message)
            : base(message)
        {
            HResult = HResults.COR_E_IO;
        }

        public IOException(string? message, int hresult)
            : base(message)
        {
            HResult = hresult;
        }

        public IOException(string? message, Exception? innerException)
            : base(message, innerException)
        {
            HResult = HResults.COR_E_IO;
        }

        [Obsolete(Obsoletions.LegacyFormatterImplMessage, DiagnosticId = Obsoletions.LegacyFormatterImplDiagId, UrlFormat = Obsoletions.SharedUrlFormat)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected IOException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
