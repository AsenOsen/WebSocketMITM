using Fleck;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace SupremaSniffer
{
    internal class MitmServer
    {
        private WebSocketServer mitmServer;
        private Dictionary<int, RealServer> realConnections = new Dictionary<int, RealServer>();
        private SocketMessageDelegate requestCallback, responseCallback;

        public delegate void SocketMessageDelegate(string host, byte[] data);

        public MitmServer(string host = "127.0.0.1", int port = 443)
        {
            mitmServer = new WebSocketServer("wss://" + host + ":" + port);
            mitmServer.Certificate = getMitmCertificate();
        }

        public void run()
        {
            mitmServer.Start(socket =>
            {
                socket.OnOpen = () => onOpen(socket);
                socket.OnClose = () => onClose(socket);
                socket.OnError = error => onError(socket);
                socket.OnBinary = data => onRequest(socket, data);
                socket.OnMessage = data => onRequest(socket, Encoding.UTF8.GetBytes(data));
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

        private void onRequest(IWebSocketConnection socket, byte[] data)
        {
            if (!realConnections.ContainsKey(socket.ConnectionInfo.ClientPort))
                realConnections[socket.ConnectionInfo.ClientPort] = new RealServer(socket, this);
            realConnections[socket.ConnectionInfo.ClientPort].onRequest(data);
            if (requestCallback != null)
            {
                requestCallback(socket.ConnectionInfo.Host, data);
            }
        }

        public void onResponse(IWebSocketConnection socket, byte[] data)
        {
            socket.Send(data);
            if (responseCallback != null)
            {
                responseCallback(socket.ConnectionInfo.Host, data);
            }
        }
    }
}
