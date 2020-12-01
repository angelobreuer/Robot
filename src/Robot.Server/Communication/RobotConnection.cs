namespace Robot.Server.Communication
{
    using System;
    using System.Buffers;
    using System.Net.Sockets;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using BufferIO.Util;
    using Microsoft.Extensions.Logging;
    using Robot.Server.Communication.Payloads;

    public class RobotConnection : IDisposable
    {
        private static readonly ReadOnlyMemory<byte> MagicSequence = new(
            new byte[] { (byte)'R', (byte)'b', (byte)'t', (byte)'C', (byte)'l', (byte)'t', });

        private readonly ILogger<RobotConnection> _logger;
        private readonly Task _receiveTask;

        public RobotConnection(Socket socket, ILogger<RobotConnection> logger)
        {
            Socket = socket ?? throw new ArgumentNullException(nameof(socket));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _receiveTask = Task.Factory.StartNew(function: Callback).Unwrap();
        }

        public Socket Socket { get; }

        /// <inheritdoc/>
        public void Dispose()
        {
            Socket.Close();
            _receiveTask.Wait();
        }

        public async Task SendAsync(OpCode opCode, ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Sending payload {OpCodeValue} {OpCode} ({Bytes} bytes) to {Endpoint}.",
                (byte)opCode, opCode, buffer.Length, Socket.RemoteEndPoint);

            if (buffer.IsEmpty)
            {
                var data = new byte[3] { (byte)opCode, 0, 0 };
                await Socket.SendAsync(data, SocketFlags.None, cancellationToken);
                return;
            }

            if (buffer.Length > ushort.MaxValue)
            {
                throw new InvalidOperationException("Payload too long.");
            }

            var payloadBuffer = ArrayPool<byte>.Shared.Rent(buffer.Length + 3);

            try
            {
                payloadBuffer[0] = (byte)opCode;
                BigEndian.GetBytes(payloadBuffer, (ushort)buffer.Length, byteOffset: 1);

                buffer.CopyTo(payloadBuffer.AsMemory(3));

                await Socket.SendAsync(payloadBuffer.AsMemory(0, buffer.Length + 3), SocketFlags.None, cancellationToken);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(payloadBuffer);
            }
        }

        private unsafe static ValueTask HandleEstablishAsync(Memory<byte> buffer)
        {
            var payload = Unsafe.As<byte, EstablishPayload>(ref buffer.Span[0]);

            if (!new Span<byte>(payload.MagicSequence, 6).SequenceEqual(MagicSequence.Span))
            {
                // magic header invalid
                throw new InvalidOperationException("Magic sequence invalid");
            }

            return default;
        }

        private async Task Callback()
        {
            while (Socket.Connected)
            {
                var headerBuffer = new byte[sizeof(byte) + sizeof(ushort)];
                var length = await Socket.ReceiveAsync(headerBuffer, SocketFlags.None);

                if (length < 3)
                {
                    return;
                }

                var opCode = (OpCode)headerBuffer[0];
                var payloadLength = BigEndian.ToUInt16(headerBuffer, offset: 1);
                var payloadBuffer = ArrayPool<byte>.Shared.Rent(payloadLength);

                _logger.LogDebug("Received payload {OpCodeValue} {OpCode} ({Bytes} bytes) from {Endpoint}.",
                    (byte)opCode, opCode, payloadLength, Socket.RemoteEndPoint);

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

        private async ValueTask HandlePingAsync()
        {
            await SendAsync(OpCode.Pong, default);
        }

        private async ValueTask HandleSensorSyncAsync(Memory<byte> buffer)
        {
            var id = buffer.Span[0];
            var value = buffer.Span[1];
        }

        private async ValueTask HandleStatusUpdateAsync(Memory<byte> buffer)
        {
            var payload = Unsafe.As<byte, StatusPayload>(ref buffer.Span[0]);
            _logger.LogInformation("Status updated: {Status}", payload.Status);
        }

        private ValueTask ProcessPayloadAsync(OpCode opCode, Memory<byte> buffer) => opCode switch
        {
            OpCode.Establish => HandleEstablishAsync(buffer),
            OpCode.SensorSync => HandleSensorSyncAsync(buffer),
            OpCode.Ping => HandlePingAsync(),
            OpCode.Status => HandleStatusUpdateAsync(buffer),
            _ => default,
        };
    }
}
