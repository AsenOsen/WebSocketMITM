using LowLevelDesign.Hexify;

namespace SupremaSniffer
{
    internal class SocketMessage
    {
        private string message;
        private byte[] binary;

        public SocketMessage(byte[] data) => binary = data;

        public SocketMessage(string data) => message = data;

        public bool isBinary
        {
            get => binary != null;
        }

        public string Message
        {
            get => message;
        }

        public byte[] Binary
        {
            get => binary;
        }

        public override string ToString()
        {
            return isBinary ? Hex.ToHexString(Binary) : Message;
        }
    }
}
