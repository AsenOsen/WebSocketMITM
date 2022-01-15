using DNS.Client;
using Fleck;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using Websocket.Client;

namespace SupremaSniffer
{
    internal class RealServer
    {
        private IWebSocketConnection mitmConnection;
        private MitmServer mitmServer;
        private WebsocketClient realSocket;
        private Queue<byte[]> forwardedMessages = new Queue<byte[]>();

        public RealServer(IWebSocketConnection connection, MitmServer mitmServer)
        {
            this.mitmConnection = connection;
            this.mitmServer = mitmServer;
            var url = new Uri("wss://" + HostToRealIP(connection.ConnectionInfo.Host) + connection.ConnectionInfo.Path);
            ClientWebSocket nativeSocket = new ClientWebSocket();
            nativeSocket.Options.SetRequestHeader("Host", connection.ConnectionInfo.Host);
            //nativeSocket.Options.SetRequestHeader("Origin", "http://"+socket.ConnectionInfo.Host+":443");
            nativeSocket.Options.RemoteCertificateValidationCallback = delegate { return true; };
            realSocket = new WebsocketClient(url, new Func<ClientWebSocket>(() => nativeSocket));
            realSocket.ReconnectTimeout = TimeSpan.FromSeconds(5);
            realSocket.MessageReceived.Subscribe(onResponse);
            Run();
        }

        private string HostToRealIP(string host)
        {
            DnsClient client = new DnsClient("8.8.8.8"); // resolve via google DNS
            ClientRequest request = client.Create();
            IList<IPAddress> ips =  (Task.Run(() => client.Lookup(host))).Result;
            return ips[0].ToString();
        }

        private async void Run()
        {
            await realSocket.Start();
            while (true)
            {
                await Task.Delay(10);
                if (forwardedMessages.Count > 0)
                {
                    realSocket.Send(forwardedMessages.Dequeue());
                }
            }
        }

        public void onRequest(byte[] data)
        {
            forwardedMessages.Enqueue(data);
        }

        private void onResponse(ResponseMessage msg)
        {
            mitmServer.onResponse(mitmConnection, msg.Binary);
        }

    }
}
