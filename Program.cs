using SupremaSniffer;

DnsPoisoner poisoner = new DnsPoisoner("127.0.0.1", new string[] { "google.com", "yahoo.com" });
poisoner.poison();

MitmServer mitmServer = new MitmServer("127.0.0.1", 444, "wss://");
mitmServer.RequestCallback = delegate (string host, SocketMessage data) {
    Console.WriteLine("\n[Request]\n" + data) ; 
};
mitmServer.ResponseCallback = delegate (string host, SocketMessage data) {
    Console.WriteLine("\n[Response]\n" + data); 
};
mitmServer.run();

while (true)
{
    Thread.Sleep(1000);
}
