using ServerSync;

var socketServer = new SyncSocketServer();
    
socketServer.StartListening("localhost", 45333);
socketServer.ReceiveMessage();

socketServer.Dispose();

Console.ReadLine();
