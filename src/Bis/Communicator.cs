using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace Bis
{
    public class Communicator : IDisposable
    {
        /// <summary>
        /// Server address
        /// </summary>
        public string Host { get; private set; }
        /// <summary>
        /// Server port
        /// </summary>
        public int Port { get; private set; } = 27696; // 'brown'
        /// <summary>
        /// Connection Timeout (ms)
        /// </summary>
        public int Timeout { get; private set; } = 5 * 1000;

        private NetworkStream _ns;
        private TcpClient _client;

        /// <summary>
        /// Constructor (using 27696 port)
        /// </summary>
        /// <param name="host">Server Address (IP adress or host name)</param>
        public Communicator(string host)
        {
            Initialize(host, Port, Timeout);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="host">Server Address (IP adress or host name)</param>
        /// <param name="port">Port</param>
        public Communicator(string host, int port)
        {
            Initialize(host, port, Timeout);
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="host">Server Address (IP adress or host name)</param>
        /// <param name="port">Port</param>
        /// <param name="timeout">Timeout (ms)</param>
        public Communicator(string host, int port, int timeout)
        {
            Initialize(host, port, timeout);
        }
        /// <summary>
        /// Open connection
        /// </summary>
        public void Open()
        {
            if(_ns != null)
            {
                Close();
            }
            TcpClient tcp = new TcpClient(Host, Port);
            _ns = tcp.GetStream();
            _ns.ReadTimeout = Timeout;
            _ns.WriteTimeout = Timeout;
            _client = new TcpClient(Host, Port);
        }
        /// <summary>
        /// Close connection
        /// </summary>
        public void Close()
        {
            if(_ns != null)
            {
                _ns.Close();
                _ns.Dispose();
            }
            if(_client != null)
            {
                _client.Close();
                _client.Dispose();
            }
        }
        /// <summary>
        ///  Synchronous communicate with server
        /// </summary>
        /// <param name="message">message</param>
        /// <returns>response</returns>
        public string Send(string message)
        {
            // send message
            Encoding enc = System.Text.Encoding.UTF8;
            byte[] sendBytes = enc.GetBytes(message + '\n');
            _ns.Write(sendBytes, 0, sendBytes.Length);

            // wait for response
            string response = "";
            using (MemoryStream ms = new MemoryStream())
            {
                byte[] resBytes = new byte[256];
                int resSize = 0;
                do
                {
                    resSize = _ns.Read(resBytes, 0, resBytes.Length);
                    if (resSize == 0)
                    {
                        Console.WriteLine("Disconnected");
                        break;
                    }
                    ms.Write(resBytes, 0, resSize);
                } while (_ns.DataAvailable || resBytes[resSize - 1] != '\n');
                response = enc.GetString(ms.GetBuffer(), 0, (int)ms.Length).TrimEnd('\n');
            }
            return response;
        }
        private void Initialize(string host, int port, int timeout)
        {
            Host = host;
            Port = port;
            Timeout = timeout;
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Close();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
