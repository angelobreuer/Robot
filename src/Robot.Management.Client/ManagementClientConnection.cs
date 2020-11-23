namespace Robot.Management.Client
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;
    using Robot.Management.Payloads;

    public class ManagementClientConnection : ManagementConnection
    {
        private static readonly byte[] Sequence = { (byte)'U', (byte)'D', (byte)'P', (byte)'M', (byte)'G', (byte)'T' };

        public ManagementClientConnection(TcpClient tcpClient) : base(tcpClient)
        {
        }

        public VideoClient VideoClient { get; private set; }

        public override Type GetPayloadType(OpCode opCode) => opCode switch
        {
            OpCode.Authenticate => throw new NotImplementedException(),
            OpCode.StartVideo => throw new NotImplementedException(),
            OpCode.StopVideo => throw new NotImplementedException(),
        };

        public override ValueTask ProcessPayloadAsync(OpCode opCode, object payload, CancellationToken cancellationToken = default)
        {
            return default;
        }

        public async Task StartVideoCaptureAsync()
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            var holePunchEndpoint = new IPEndPoint(IPAddress.Loopback, 2366);

            // do UDP hole-punch
            await socket.SendToAsync(Sequence, SocketFlags.None, holePunchEndpoint);

            var buffer = new byte[6];
            var result = await socket.ReceiveFromAsync(buffer, SocketFlags.None, holePunchEndpoint);

            if (result.ReceivedBytes < 6)
            {
                throw new InvalidOperationException("UDP hole-punch failed.");
            }

            var ipAddress = new IPAddress(buffer[..4]);
            var port = (ushort)((buffer[4] << 8) | buffer[5]);

            await SendAsync(OpCode.StartVideo, new StartVideoPayload
            {
                IpAddress = ipAddress.ToString(),
                Port = port,
            });

            VideoClient = new VideoClient(socket, holePunchEndpoint);
        }
    }
}
