namespace Robot.Communication.Server
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;

    public sealed class RobotServer : IDisposable
    {
        private readonly int _port;
        private readonly object _syncRoot;
        private Thread? _acceptThread;
        private Socket? _tcpSocket;

        public RobotServer(int port = 8080)
        {
            _syncRoot = new object();
            _port = port;
        }

        public RobotConnection? ActiveConnection { get; private set; }

        /// <inheritdoc/>
        public void Dispose()
        {
            Stop();
        }

        public void Start()
        {
            if (_tcpSocket is null)
            {
                _tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _tcpSocket.Bind(new IPEndPoint(IPAddress.Any, _port)); // TODO
                _tcpSocket.Listen(2);
            }

            if (_acceptThread is null)
            {
                _acceptThread = new Thread(AcceptCallback);
                _acceptThread.Start();
                _acceptThread.Name = "Robot Server Accept Thread";
            }
        }

        public void Stop()
        {
            ActiveConnection?.Socket.Close();

            _tcpSocket?.Dispose();
            _tcpSocket = null;

            if (_acceptThread is not null && !_acceptThread.Join(2000))
            {
                throw new InvalidOperationException("Accept Thread did not exit in time.");
            }
        }

        private void AcceptCallback(object? obj)
        {
            while (_tcpSocket is not null)
            {
                var socket = _tcpSocket.Accept();

                lock (_syncRoot)
                {
                    ActiveConnection?.Socket.Close();
                    ActiveConnection = new RobotConnection(socket);
                }
            }
        }
    }
}
