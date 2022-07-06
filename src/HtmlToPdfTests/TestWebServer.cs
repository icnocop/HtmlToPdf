// <copyright file="TestWebServer.cs" company="HtmlToPdf">
// Copyright (c) HtmlToPdf. All rights reserved.
// </copyright>

namespace HtmlToPdfTests
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using Huygens;

    /// <summary>
    /// Test web server.
    /// </summary>
    /// <seealso cref="Huygens.SocketServer" />
    public class TestWebServer : SocketServer, IDisposable
    {
        private static readonly Random Random = new Random();

        /// <summary>
        /// Initializes a new instance of the <see cref="TestWebServer"/> class.
        /// </summary>
        /// <param name="virtualPath">The virtual path.</param>
        /// <param name="physicalPath">The physical path.</param>
        public TestWebServer(string virtualPath, string physicalPath)
            : base(GetAvailablePort(), virtualPath, physicalPath)
        {
            this.Start();
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            this.ShutDown();

            base.Dispose();
        }

        private static int GetAvailablePort()
        {
            return GetAvailablePort(8000, 10000);
        }

        private static int GetAvailablePort(int minPort, int maxPort)
        {
            int port = Random.Next(minPort, maxPort + 1);
            while (!IsPortAvailable(port))
            {
                port = Random.Next(minPort, maxPort + 1);
            }

            return port;
        }

        private static bool IsPortAvailable(int port)
        {
            try
            {
                TcpListener listener = new TcpListener(IPAddress.Loopback, port);
                listener.Start();

                // port is available
                listener.Stop();
                return true;
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.AddressAlreadyInUse)
            {
                return false;
            }
        }
    }
}
