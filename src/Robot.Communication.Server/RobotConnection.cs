namespace Robot.Communication.Server
{
    using System;
    using System.Buffers;
    using System.Net.Sockets;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using BufferIO.Util;
    using Robot.Communication.Structures;

    public class RobotConnection : IDisposable
    {
        private static readonly ReadOnlyMemory<byte> MagicSequence = new(
            new byte[] { (byte)'R', (byte)'b', (byte)'t', (byte)'C', (byte)'l', (byte)'t', });

        private readonly Task _receiveTask;

        public RobotConnection(Socket socket)
        {
            Socket = socket ?? throw new ArgumentNullException(nameof(socket));
            _receiveTask = Task.Factory.StartNew(function: Callback).Unwrap();
        }

        public Socket Socket { get; }

        /// <inheritdoc/>
        public void Dispose()
        {
            Socket.Close();
            _receiveTask.Wait();
        }

        public async Task SendAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken = default)
        {
        }

        private async Task Callback()
        {
            while (Socket.Connected)
            {
                var headerBuffer = new byte[sizeof(byte) + sizeof(ushort)];
                var length = await Socket.ReceiveAsync(headerBuffer, SocketFlags.None);

                if (length < 2)
                {
                    return;
                }

                var opCode = (OpCode)headerBuffer[0];
                var payloadLength = BigEndian.ToUInt16(headerBuffer, offset: 1);
                var payloadBuffer = ArrayPool<byte>.Shared.Rent(payloadLength);

                try
                {
                    length = await Socket.ReceiveAsync(payloadBuffer.AsMemory(0, payloadLength), SocketFlags.None);

                    if (length < payloadLength)
                    {
                        return;
                    }

                    await ProcessPayloadAsync(opCode, payloadBuffer.AsMemory(0, length));
                }
                catch
                {
                    // TODO
                    Socket.Close();
                    throw;
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(payloadBuffer);
                }
            }
        }

        private unsafe async Task ProcessPayloadAsync(OpCode opCode, Memory<byte> buffer)
        {
            switch (opCode)
            {
                case OpCode.Establish:
                    var payload = Unsafe.As<byte, EstablishPayload>(ref buffer.Span[0]);

                    if (!new Span<byte>(payload.MagicSequence, 6).SequenceEqual(MagicSequence.Span))
                    {
                        // magic header invalid
                        throw new InvalidOperationException("Magic sequence invalid");
                    }

                    break;

                case OpCode.SensorSync:
                    var id = buffer.Span[0];
                    var value = buffer.Span[1];
                    break;

                case OpCode.Ping:
            }
        }
    }
}
