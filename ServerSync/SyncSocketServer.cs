using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace ServerSync
{
    public class SyncSocketServer
    {

        private IPAddress? _ipAddress;
        private IPHostEntry? _ipHost;
        private IPEndPoint? _ipEndPoint;
        private Socket? _listener;


        public void StartListening(string ip, int port)
        {
            try
            {
                _ipHost = Dns.GetHostEntry(ip);

                Console.WriteLine($"ip host: {_ipHost}");
                _ipAddress = _ipHost.AddressList[0];
                _ipEndPoint = new IPEndPoint(_ipAddress, port);

                _listener = new Socket(_ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                _listener.Bind(_ipEndPoint);

                _listener.Listen(10);

                Console.WriteLine("Server Socket is listening");
            }
            catch (SocketException e)
            {
                LogError(e);
            }
        }

        public void ReceiveMessages()
        {
            if (_listener != null)
            {
                try
                {
                    var handler = _listener.Accept();

                    while (true)
                    {

                        byte[] bufferForMessageLength = new byte[4];
                        handler.Receive(bufferForMessageLength);
                        int messageLength = BitConverter.ToInt32(bufferForMessageLength, 0);

                        byte[] bufferForMessage = new byte[messageLength];
                        var received = handler.Receive(bufferForMessage);
                        var message = Encoding.UTF8.GetString(bufferForMessage, 0, received);

                        Console.WriteLine($"Message received from client: \"{message}\"");


                        if (message == "DISCONNECT")
                        {
                            Console.WriteLine("Client disconnected");
                            handler.Shutdown(SocketShutdown.Receive);
                            handler.Close();
                            Dispose();

                            break;
                        }

                        UserInteractionLoop(handler);

                    }
                }
                catch (SocketException e)
                {
                    if(e.ErrorCode == (int)SocketError.ConnectionAborted)
                    {
                        Console.WriteLine("Client disconnected abruptly");
                    }
                    else
                    {
                        LogError(e);
                    }
                }

            }
            else
            {
                Console.WriteLine($"the server wasnt initialized");
            }
        }


        private void UserInteractionLoop(Socket handler)
        {
            while (true)
            {
                Console.WriteLine($"Type your message:");
                var message = Console.ReadLine();

                if (message != null)
                {
                    SendMessage(handler, message);
                    break;
                }
            }
        }

        public void SendMessage(Socket handler, string message)
        {
            byte[] byteMessage = Encoding.UTF8.GetBytes(message);

            int messageLength = byteMessage.Length;

            byte[] byteMessageLength = BitConverter.GetBytes(messageLength);

            handler.Send(byteMessageLength);
            handler.Send(byteMessage);
        }

        public void Dispose()
        {
            _listener?.Dispose();
        }
     

        private void LogError(Exception e)
        {
            Console.WriteLine($"An error was detected: {e.Message}");
        }
    }
}
