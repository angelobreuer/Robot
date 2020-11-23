namespace Robot.Management
{
    using System;
    using System.IO;
    using System.Net.Sockets;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;

    public abstract class ManagementConnection
    {
        private readonly NetworkStream _networkStream;

        public ManagementConnection(TcpClient tcpClient)
        {
            TcpClient = tcpClient ?? throw new ArgumentNullException(nameof(tcpClient));
            _networkStream = TcpClient.GetStream();

            _ = RunReceiveLoopAsync();
        }

        public bool IsConnected => TcpClient.Connected;

        public TcpClient TcpClient { get; }

        public abstract Type GetPayloadType(OpCode opCode);

        public abstract ValueTask ProcessPayloadAsync(OpCode opCode, object? payload, CancellationToken cancellationToken = default);

        public async Task RunReceiveLoopAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            while (TcpClient.Connected && !cancellationToken.IsCancellationRequested)
            {
                var headerBuffer = new byte[5];

                // read header (5 bytes)
                var headerLength = await _networkStream.ReadAsync(headerBuffer.AsMemory(0, headerBuffer.Length), cancellationToken);

                if (headerLength < 5)
                {
                    throw new EndOfStreamException();
                }

                // decode payload header
                var length = (headerBuffer[1] << 24) | (headerBuffer[2] << 16) | (headerBuffer[3] << 8) | headerBuffer[4];
                var opCode = (OpCode)headerBuffer[0];

                using var stream = new PayloadReadStream(_networkStream, length);
                var payload = await JsonSerializer.DeserializeAsync(stream, GetPayloadType(opCode), options: null, cancellationToken);
                await ProcessPayloadAsync(opCode, payload, cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task SendAsync(OpCode opCode, object? payload, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using var memoryStream = new MemoryStream
            {
                Capacity = 5 /* Header */ + 1024 /* Avg. Payload Size */,
                Position = 5, // Write payload content first
            };

            SerializePayload(memoryStream, payload);
            var payloadLength = memoryStream.Position - 5;

            memoryStream.Position = 0;
            memoryStream.WriteByte((byte)opCode);
            memoryStream.WriteByte((byte)(payloadLength >> 24));
            memoryStream.WriteByte((byte)(payloadLength >> 16));
            memoryStream.WriteByte((byte)(payloadLength >> 8));
            memoryStream.WriteByte((byte)payloadLength);

            memoryStream.Position += payloadLength;

            memoryStream.TryGetBuffer(out var buffer);
            await _networkStream.WriteAsync(buffer, cancellationToken);
        }

        private static void SerializePayload(Stream stream, object? payload)
        {
            using var utf8JsonWriter = new Utf8JsonWriter(stream);

            if (payload is null)
            {
                utf8JsonWriter.WriteNullValue();
            }
            else
            {
                JsonSerializer.Serialize(utf8JsonWriter, payload, payload.GetType());
            }
        }
    }
}
