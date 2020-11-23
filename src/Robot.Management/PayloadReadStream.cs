namespace Robot.Management
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    internal sealed class PayloadReadStream : Stream
    {
        private readonly Stream _stream;
        private int _remainingBytes;
        private int _totalBytesRead;

        public PayloadReadStream(Stream stream, int length)
        {
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));
            _remainingBytes = length;
        }

        /// <inheritdoc/>
        public override bool CanRead => _remainingBytes is not 0;

        /// <inheritdoc/>
        public override bool CanSeek => false;

        /// <inheritdoc/>
        public override bool CanWrite => false;

        /// <inheritdoc/>
        public override long Length => _remainingBytes + _totalBytesRead;

        /// <inheritdoc/>
        public override long Position
        {
            get => _totalBytesRead;
            set => throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override void Flush()
        {
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_remainingBytes <= 0 || buffer.Length == 0)
            {
                return 0;
            }

            if (buffer.Length > _remainingBytes)
            {
                count -= _remainingBytes;
            }

            var bytesRead = _stream.Read(buffer, offset, count);

            if (bytesRead <= 0)
            {
                throw new EndOfStreamException();
            }

            _remainingBytes -= bytesRead;
            _totalBytesRead += bytesRead;

            return bytesRead;
        }

        /// <inheritdoc/>
        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_remainingBytes <= 0 || buffer.Length == 0)
            {
                return 0;
            }

            if (buffer.Length > _remainingBytes)
            {
                count -= _remainingBytes;
            }

            var bytesRead = await _stream
                .ReadAsync(buffer.AsMemory(offset, count), cancellationToken)
                .ConfigureAwait(false);

            if (bytesRead <= 0)
            {
                throw new EndOfStreamException();
            }

            _remainingBytes -= bytesRead;
            _totalBytesRead += bytesRead;

            return bytesRead;
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
    }
}
