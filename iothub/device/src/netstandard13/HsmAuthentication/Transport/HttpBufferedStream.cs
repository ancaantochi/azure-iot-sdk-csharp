// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.Devices.Client.HsmAuthentication.Transport
{
    internal class HttpBufferedStream : Stream
    {
        private const char CR = '\r';
        private const char LF = '\n';
        private const int BufferSize = 2048;
        private readonly Stream _innerStream;
        private readonly byte[] _innerBuffer;
        private int _bufferOffset;
        private int _bufferCount;

        public HttpBufferedStream(Stream stream)
        {
            _innerStream = stream;
            _innerBuffer = new byte[BufferSize];
        }

        public override void Flush()
        {
            _innerStream.Flush();
        }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            return _innerStream.FlushAsync(cancellationToken);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_bufferCount == 0)
            {
                _bufferOffset = 0;
                _bufferCount = _innerStream.Read(_innerBuffer, _bufferOffset, BufferSize);
                
            }
            return ReadInternalBuffer(buffer, offset, count);
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (_bufferCount == 0)
            {
                _bufferOffset = 0;
                _bufferCount = await _innerStream.ReadAsync(_innerBuffer, _bufferOffset, BufferSize, cancellationToken);
            }

            return ReadInternalBuffer(buffer, offset, count);
        }

        public async Task<string> ReadLineAsync(CancellationToken cancellationToken)
        {
            int position = 0;
            byte[] buffer = new byte[1];
            bool crFound = false;
            var builder = new StringBuilder();
            while (true)
            {
                var length = await this.ReadAsync(buffer, 0, buffer.Length, cancellationToken)
                    .ConfigureAwait(false);

                if (length == 0)
                {
                    throw new IOException("Unexpected end of stream.");
                }

                if (crFound && (char)buffer[position] == LF)
                {
                    builder.Remove(builder.Length - 1, 1);
                    return builder.ToString();
                }

                builder.Append((char)buffer[position]);
                crFound = (char)buffer[position] == CR;
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _innerStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _innerStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _innerStream.Write(buffer, offset, count);
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return _innerStream.WriteAsync(buffer, offset, count, cancellationToken);
        }

        public override bool CanRead => _innerStream.CanRead || _bufferCount > 0;

        public override bool CanSeek => _innerStream.CanSeek;

        public override bool CanWrite => _innerStream.CanWrite;

        public override long Length => _innerStream.Length;

        public override long Position
        {
            get => _innerStream.Position;
            set => _innerStream.Position = value;
        }

        protected override void Dispose(bool disposing)
        {
            _innerStream.Dispose();
        }

        private int ReadInternalBuffer(byte[] buffer, int offset, int count)
        {
            if (_bufferCount > 0)
            {
                int copyBytesCount = Math.Min(_bufferCount, count);
                Buffer.BlockCopy(_innerBuffer, _bufferOffset, buffer, offset, copyBytesCount);
                _bufferOffset += copyBytesCount;
                _bufferCount -= copyBytesCount;
                return copyBytesCount;
            }

            return 0;
        }
    }
}
