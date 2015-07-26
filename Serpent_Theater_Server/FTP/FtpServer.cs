using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using log4net;

namespace Serpent_Theater_Server.FTP
{
    public class FtpServer : IDisposable
    {
        readonly ILog _log = LogManager.GetLogger(typeof(FtpServer));

        private bool _disposed;
        private bool _listening;

        private TcpListener _listener;
        private List<ClientConnection> _activeConnections;

        private readonly IPEndPoint _localEndPoint;

        public FtpServer() : this(IPAddress.IPv6Any, 21)
        {

        }

        public FtpServer(IPAddress ipAddress, int port)
        {
            _localEndPoint = new IPEndPoint(ipAddress, port);
        }

        public void Start()
        {
            _listener = new TcpListener(_localEndPoint);
            _listener.Server.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
            _log.Info("#Version: 1.0");
            _log.Info("#Fields: date time c-ip c-port cs-username cs-method cs-uri-stem sc-status sc-bytes cs-bytes s-name s-port");

            _listening = true;
            _listener.Start();

            _activeConnections = new List<ClientConnection>();

            _listener.BeginAcceptTcpClient(HandleAcceptTcpClient, _listener);
        }

        public void Stop()
        {
            _log.Info("Stopping FtpServer");

            _listening = false;
            _listener.Stop();

            _listener = null;
        }

        private void HandleAcceptTcpClient(IAsyncResult result)
        {
            if (!_listening) return;
            _listener.BeginAcceptTcpClient(HandleAcceptTcpClient, _listener);

            var client = _listener.EndAcceptTcpClient(result);

            var connection = new ClientConnection(client);

            _activeConnections.Add(connection);

            ThreadPool.QueueUserWorkItem(connection.HandleClient, client);
        }

        public bool IsActive()
        {
            return _listening;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Stop();

                    foreach (var conn in _activeConnections)
                    {
                        conn.Dispose();
                    }
                }
            }
            _disposed = true;
        }
    }
}
