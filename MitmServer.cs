using Fleck;
using System.Security.Cryptography.X509Certificates;

namespace SupremaSniffer
{
    internal class MitmServer
    {
        private WebSocketServer mitmServer;
        private Dictionary<int, RealServer> realConnections = new Dictionary<int, RealServer>();
        private SocketMessageDelegate requestCallback, responseCallback;
        private string socketProtocol;

        public delegate void SocketMessageDelegate(string host, SocketMessage data);

        public MitmServer(string host = "127.0.0.1", int port = 443, string protocol = "wss://")
        {
            socketProtocol = protocol;
            mitmServer = new WebSocketServer(protocol + host + ":" + port);
            mitmServer.Certificate = getMitmCertificate();
        }

        public void run()
        {
            mitmServer.Start(socket =>
            {
                socket.OnOpen = () => onOpen(socket);
                socket.OnClose = () => onClose(socket);
                socket.OnError = error => onError(socket);
                socket.OnBinary = data => onRequest(socket, new SocketMessage(data));
                socket.OnMessage = data => onRequest(socket, new SocketMessage(data));
            });
        }

        public SocketMessageDelegate ResponseCallback { set => responseCallback = value; }

        public SocketMessageDelegate RequestCallback { set => requestCallback = value; }

        private X509Certificate2 getMitmCertificate()
        {
            Stream certificateStream = new FileStream(@"certificate.pfx", FileMode.Open);
            var certificateBytes = new byte[certificateStream.Length];
            certificateStream.Read(certificateBytes, 0, certificateBytes.Length);
            return new X509Certificate2(certificateBytes, "mitmcert");
        }

        private void onOpen(IWebSocketConnection socket)
        {
            Console.WriteLine("MITM: new conn");
        }

        private void onClose(IWebSocketConnection socket)
        {
            Console.WriteLine("MITM: closed");
        }

        private void onError(IWebSocketConnection socket)
        {
            Console.WriteLine("MITM: error");
        }

        private void onRequest(IWebSocketConnection socket, SocketMessage data)
        {
            if (!realConnections.ContainsKey(socket.ConnectionInfo.ClientPort))
                realConnections[socket.ConnectionInfo.ClientPort] = new RealServer(socket, this, socketProtocol);
            realConnections[socket.ConnectionInfo.ClientPort].onRequest(data);
            if (requestCallback != null)
            {
                requestCallback(socket.ConnectionInfo.Host, data);
            }
        }

        public void onResponse(IWebSocketConnection socket, SocketMessage data)
        {
            if (data.isBinary)
            {
                socket.Send(data.Binary);
            } else
            {
                socket.Send(data.Message);
            }
            if (responseCallback != null)
            {
                responseCallback(socket.ConnectionInfo.Host, data);
            }
        }
    }
}
