namespace Robot.Management.Client
{
    using System;
    using System.Drawing;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading.Tasks;

    public sealed class VideoClient
    {
        private readonly byte[] _receiveBuffer;
        private readonly Socket _videoClient;
        private readonly IPEndPoint _endPoint;

        public VideoClient(Socket socket, IPEndPoint endPoint)
        {
            _receiveBuffer = GC.AllocateUninitializedArray<byte>(1 * 1024 * 1024);
            _videoClient = socket;
            _endPoint = endPoint;
        }

        public async Task<Bitmap> ReceiveAsync()
        {
            var result = await _videoClient.ReceiveFromAsync(_receiveBuffer, SocketFlags.None, _endPoint);
            using var memoryStream = new MemoryStream(_receiveBuffer, 0, result.ReceivedBytes);
            return new Bitmap(memoryStream);
        }
    }
}
