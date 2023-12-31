// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Net.Http
{
    internal abstract class HttpContentStream : HttpBaseStream
    {
        protected internal HttpConnection? _connection;

        public HttpContentStream(HttpConnection connection)
        {
            _connection = connection;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            ValidateBufferArguments(buffer, offset, count);
            Write(new ReadOnlySpan<byte>(buffer, offset, count));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_connection != null)
                {
                    _connection.Dispose();
                    _connection = null;
                }
            }

            base.Dispose(disposing);
        }

        protected HttpConnection GetConnectionOrThrow()
        {
            HttpConnection? c = _connection;

            // Disposal should only ever happen if the user-code that was handed this instance disposed of
            // it, which is misuse, or held onto it and tried to use it later after we've disposed of it,
            // which is also misuse.
            ObjectDisposedException.ThrowIf(c is null, this);

            return c;
        }
    }
}
