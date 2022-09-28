using DNS.Client;
using Fleck;
using System.Net;
using System.Net.WebSockets;
using Websocket.Client;

namespace SupremaSniffer
{
    internal class RealServer
    {
        private IWebSocketConnection mitmConnection;
        private MitmServer mitmServer;
        private WebsocketClient realSocket;
        private Queue<SocketMessage> forwardedMessages = new Queue<SocketMessage>();

        public RealServer(IWebSocketConnection connection, MitmServer mitmServer, string protocol)
        {
            this.mitmConnection = connection;
            this.mitmServer = mitmServer;
            var url = new Uri(protocol + HostToRealIP(connection.ConnectionInfo.Host) + connection.ConnectionInfo.Path);
            ClientWebSocket nativeSocket = new ClientWebSocket();
            nativeSocket.Options.SetRequestHeader("Host", connection.ConnectionInfo.Host);
            //nativeSocket.Options.SetRequestHeader("Origin", "http://"+socket.ConnectionInfo.Host+":443");
            nativeSocket.Options.RemoteCertificateValidationCallback = delegate { return true; };
            realSocket = new WebsocketClient(url, new Func<ClientWebSocket>(() => nativeSocket));
            realSocket.ReconnectTimeout = TimeSpan.FromSeconds(60);
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
                    SocketMessage msg = forwardedMessages.Dequeue();
                    if (msg.isBinary)
                    {
                        realSocket.Send(msg.Binary);
                    } else
                    {
                        realSocket.Send(msg.Message);
                    }
                }
            }
        }

        public void onRequest(SocketMessage data)
        {
            forwardedMessages.Enqueue(data);
        }

        private void onResponse(ResponseMessage msg)
        {
            if (msg.MessageType == WebSocketMessageType.Binary)
            {
                mitmServer.onResponse(mitmConnection, new SocketMessage(msg.Binary));
            }
            if (msg.MessageType == WebSocketMessageType.Text)
            {
                mitmServer.onResponse(mitmConnection, new SocketMessage(msg.Text));
            }
        }

    }
}
