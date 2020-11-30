namespace Robot.Communication
{
    using System;
    using System.Buffers;
    using System.Net.Sockets;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Threading.Tasks;

    public class Connection
    {
        private readonly Socket _socket;

        private static unsafe void WritePayload<T>(byte[] buffer, T payload) where T : struct
        {
            fixed (byte* bufferPtr = buffer)
            {
                Marshal.PtrToStructure(new(bufferPtr), payload);
            }
        }

        public async Task SendPayloadAsync<T>(T payload, CancellationToken cancellationToken = default) where T : struct
        {
            cancellationToken.ThrowIfCancellationRequested();

            var size = Marshal.SizeOf(payload);
            var pooledBuffer = ArrayPool<byte>.Shared.Rent(size);

            try
            {
                WritePayload(pooledBuffer, payload);
                await _socket.SendAsync(pooledBuffer.AsMemory(0, size), SocketFlags.None, cancellationToken);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(pooledBuffer);
            }
        }
    }
}
