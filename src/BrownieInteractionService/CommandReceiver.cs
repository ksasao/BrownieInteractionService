using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BisCore
{
    public class CommandReceiver
    {
        /// <summary>
        /// Server port
        /// </summary>
        public int Port { get; private set; } = 27696; // 'brown'

        /// <summary>
        /// Read timeout
        /// </summary>
        public int ReadTimeout { get; set; } = 5000;

        TcpListener listener = null;
        CancellationTokenSource _tokenSource = null;

        /// <summary>
        /// Receive bis.exe command
        /// </summary>
        public CommandReceiver()
        {
            // use default port
            Initialize(Port);
        }

        public CommandReceiver(int port)
        {
            Initialize(port);
        }

        private void Initialize(int port)
        {
            Port = port;
        }
        /// <summary>
        /// Open receiver
        /// </summary>
        public void Open()
        {
            // Listen for IPv4 & IPv6
            listener = new TcpListener(IPAddress.IPv6Any, Port);
            listener.Server.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, 0);
            listener.Start();
            MainLoop();
        }
        /// <summary>
        /// Close receiver
        /// </summary>
        public void Close()
        {
            _tokenSource.Cancel();
            if(listener != null)
            {
                listener.Stop();
            }
        }
        private void MainLoop()
        {
            byte[] bytes = new byte[256];

            if (_tokenSource == null) _tokenSource = new CancellationTokenSource();
            var cancellationToken = _tokenSource.Token;
            Console.Write("Waiting for a connection... ");

            Task.Factory.StartNew( () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {

                    Encoding enc = Encoding.UTF8;
                    bool disconnected = false;
                    System.IO.MemoryStream ms = new System.IO.MemoryStream();
                    byte[] resBytes = new byte[256];
                    int resSize = 0;

                    using (TcpClient client = listener.AcceptTcpClient())
                    {
                        using (NetworkStream ns = client.GetStream())
                        {
                            ns.ReadTimeout = Timeout.Infinite;
                            do
                            {
                                try
                                {
                                    resSize = ns.Read(resBytes, 0, resBytes.Length);
                                    if (resSize == 0)
                                    {
                                        disconnected = true;
                                        Console.WriteLine("Client disconnected.");
                                        break;
                                    }
                                    ms.Write(resBytes, 0, resSize);
                                    ns.ReadTimeout = ReadTimeout;
                                }catch(System.IO.IOException ex)
                                {
                                    Console.WriteLine(ex.Message);
                                }
                            } while (ns.DataAvailable || resBytes[resSize - 1] != '\n');
                            string message = enc.GetString(ms.GetBuffer(), 0, (int)ms.Length);
                            ms.Close();
                            message = message.TrimEnd('\n');
                            Console.WriteLine(message);

                            if (!disconnected)
                            {
                                message = $"Received: {message}";
                                byte[] sendBytes = enc.GetBytes(message + '\n');
                                ns.Write(sendBytes, 0, sendBytes.Length);
                                Console.WriteLine(message);
                            }
                            ns.Close();
                        }
                    }
                }
            }, cancellationToken);
        }
    }
}
