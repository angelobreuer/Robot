namespace Robot.Server.Management
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;
    using Robot.Management;
    using Robot.Management.Payloads;

    public sealed class ManagementClientConnection : ManagementConnection
    {
        private IPEndPoint? _videoEndpoint;
        private Socket _videoSocket;

        public ManagementClientConnection(TcpClient tcpClient) : base(tcpClient)
        {
        }

        /// <inheritdoc/>
        public override Type GetPayloadType(OpCode opCode) => opCode switch
        {
            OpCode.StartVideo => typeof(StartVideoPayload),
            OpCode.StopVideo => throw new NotImplementedException(),
            _ => throw new InvalidOperationException(),
        };

        /// <inheritdoc/>
        public override ValueTask ProcessPayloadAsync(OpCode opCode, object rawPayload, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            switch (rawPayload)
            {
                case StartVideoPayload payload:
                    _videoSocket?.Disconnect(true);
                    _videoSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    _videoEndpoint = new IPEndPoint(IPAddress.Parse(payload.IpAddress), payload.Port);
                    break;

                case null when opCode is OpCode.StopVideo:
                    _videoSocket.Close();
                    _videoSocket = null;
                    break;
            }

            return default;
        }

        public async ValueTask SendFrameAsync(ArraySegment<byte> data)
        {
            if (_videoSocket is null)
            {
                return;
            }

            await _videoSocket.SendToAsync(data, SocketFlags.None, _videoEndpoint);
        }
    }
}
