using Client;

var client = new SyncSocketClient();

client.ConnectToEndPoint("localhost", 45333);

client.UserInteractionLoop();
