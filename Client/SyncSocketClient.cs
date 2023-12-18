using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace Client
{
    public class SyncSocketClient
    {

        private IPAddress? _ipAddress;
        private IPHostEntry? _ipHost;
        private IPEndPoint? _ipEndPoint;
        private Socket? _client;

        public void ConnectToEndPoint(string ip, int port)
        {
            try
            {
                _ipHost = Dns.GetHostEntry(ip);
                _ipAddress = _ipHost.AddressList[0];
                _ipEndPoint = new IPEndPoint(_ipAddress, port);

                _client = new Socket(_ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                _client.Connect(_ipEndPoint);
                Console.WriteLine("Socket connected to endpoint");

            }
            catch (SocketException e)
            {
                LogError(e);
            }
            catch (Exception e)
            {
                LogError(e);
            }
        }


        public void SendMessage(string message)
        {
            if (_client != null)
            {
                try
                {
                    byte[] messageBytes = Encoding.UTF8.GetBytes(message);

                    int messageLength = messageBytes.Length;
                    byte[] messageLengthBytes = BitConverter.GetBytes(messageLength);
                    _client.Send(messageLengthBytes);
                    _client.Send(messageBytes);
                }
                catch (SocketException e)
                {
                    if (e.ErrorCode == (int)SocketError.ConnectionAborted)
                    {
                        Console.WriteLine("server disconnected abruptly");
                    }
                    else
                    {
                        LogError(e);
                    }
                }
            }
            else
            {
                Console.WriteLine($"The client server wasnt initialized");
            }

        }

        public void ReceiveMessage()
        {
            if (_client != null)
            {
                try
                {
                    var bufferForMessageLength = new byte[4];
                    _client.Receive(bufferForMessageLength);
                    var messageLength = BitConverter.ToInt32(bufferForMessageLength, 0);

                    byte[] buffer = new byte[messageLength];
                    var received = _client.Receive(buffer);
                    var message = Encoding.UTF8.GetString(buffer, 0, received);

                    Console.WriteLine($"Message received from Server: {message}");

                }
                catch (SocketException e)
                {
                    if (e.ErrorCode == (int)SocketError.ConnectionAborted)
                    {
                        Console.WriteLine("server disconnected abruptly");
                    }
                    else
                    {
                        LogError(e);
                    }
                }
            }
            else
            {
                Console.WriteLine($"the client server wasnt initialized");
            }
        }

        public void UserInteractionLoop()
        {
            while (true)
            {
                Console.WriteLine("Type your message or type END to terminate:");
                var message = Console.ReadLine();

                if (message == null || message == "END")
                {
                    this.Disconnect();
                    this.Dispose();
                    break;
                }
                else
                {
                    this.SendMessage(message);
                    this.ReceiveMessage();
                }
            }
        }

        public void Disconnect()
        {
            try
            {
                var disconnectMessage = "DISCONNECT";
                SendMessage(disconnectMessage);

                _client?.Shutdown(SocketShutdown.Send);
                _client?.Close();

                Console.WriteLine("Disconnected");
            }
            catch (SocketException e) {
                LogError(e);
            }
        }

        private void LogError(Exception e)
        {
            Console.WriteLine($"An error was detected: {e.Message}");
        }

        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}
