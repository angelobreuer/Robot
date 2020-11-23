namespace Robot.Server.Management
{
    using System;
    using System.Collections.Generic;
    using System.Net.Sockets;
    using System.Threading;

    public sealed class ManagementServer : IDisposable
    {
        private readonly List<ManagementClientConnection> _connections;
        private readonly object _connectionsLock;
        private readonly Thread _listenerThread;
        private readonly TcpListener _tcpListener;
        private bool _running;

        public ManagementServer()
        {
            _tcpListener = TcpListener.Create(4652);
            _tcpListener.Start();

            _connections = new List<ManagementClientConnection>();
            _connectionsLock = new object();

            _running = true;

            _listenerThread = new Thread(Run);
            _listenerThread.Start();
            _listenerThread.Name = "Management Server Thread";
        }

        public IEnumerable<ManagementClientConnection> Connections
        {
            get
            {
                lock (_connectionsLock)
                {
                    return _connections.ToArray();
                }
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (!_running)
            {
                return;
            }

            _running = false;
            _tcpListener.Stop();
            _listenerThread.Join();
        }

        private void Run(object? state)
        {
            while (_running)
            {
                try
                {
                    var client = _tcpListener.AcceptTcpClient();

                    lock (_connectionsLock)
                    {
                        _connections.Add(new ManagementClientConnection(client));
                    }
                }
                catch (Exception)
                {
                    // TODO
                }
            }
        }
    }
}
