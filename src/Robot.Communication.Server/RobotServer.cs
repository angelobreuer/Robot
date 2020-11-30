namespace Robot.Communication.Server
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using Microsoft.Extensions.Logging;

    public sealed class RobotServer : IDisposable
    {
        private readonly object _syncRoot;
        private readonly ILogger<RobotServer> _logger;
        private readonly ILogger<RobotConnection> _connectionLogger;
        private Thread? _acceptThread;
        private Socket? _tcpSocket;

        public RobotServer(ILogger<RobotServer> logger, ILogger<RobotConnection> connectionLogger)
        {
            _syncRoot = new object();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _connectionLogger = connectionLogger ?? throw new ArgumentNullException(nameof(connectionLogger));
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
                _tcpSocket.Bind(new IPEndPoint(IPAddress.Any, 8080));
                _tcpSocket.Listen(2);

                _logger.LogInformation("Started server at *:8080");
            }

            if (_acceptThread is null)
            {
                _acceptThread = new Thread(AcceptCallback);
                _acceptThread.Start();
                _acceptThread.Name = "Robot Server Accept Thread";

                _logger.LogInformation("Listening for connections...");
            }
        }

        public void Stop()
        {
            _logger.LogInformation("Stopping server...");

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

                _logger.LogInformation("Accepted connection from {Endpoint}.", socket.RemoteEndPoint);

                lock (_syncRoot)
                {
                    ActiveConnection?.Socket.Close();
                    ActiveConnection = new RobotConnection(socket, _connectionLogger);
                }
            }
        }
    }
}
