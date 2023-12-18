using Client;

var client = new SyncSocketClient();

client.ConnectToEndPoint("localhost", 45333);

client.SendMessage("I love you socket server.");
client.ReceiveMessage();
client.SendMessage("My cat wrote that.");
client.ReceiveMessage();

client.Disconnect();

client.Dispose();

Console.ReadLine();
