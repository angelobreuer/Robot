namespace Robot.Server
{
    using System.Linq;
    using System.Net.Sockets;
    using System.Threading.Tasks;

    internal sealed class UdpHolePuncher
    {
        private static readonly byte[] Sequence = { (byte)'U', (byte)'D', (byte)'P', (byte)'M', (byte)'G', (byte)'T' };
        private readonly UdpClient _socket;

        public UdpHolePuncher()
        {
            _socket = new UdpClient(2366);
            _ = RunAsync();
        }

        public async Task ProcessNextAsync()
        {
            var result = await _socket.ReceiveAsync();

            if (result.Buffer.SequenceEqual(Sequence))
            {
                var response = new byte[4 + 2];
                result.RemoteEndPoint.Address.GetAddressBytes().CopyTo(response, 0);

                var port = result.RemoteEndPoint.Port;

                response[4] = (byte)(port >> 8);
                response[5] = (byte)port;

                await _socket.SendAsync(response, response.Length, result.RemoteEndPoint);
            }
        }

        public async Task RunAsync()
        {
            while (true)
            {
                await ProcessNextAsync();
            }
        }
    }
}
