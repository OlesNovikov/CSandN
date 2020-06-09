using System;
using System.Net;

namespace MessageClasses
{
    [Serializable]
    public class TcpRequestMessage : Message
    {
        public string clientName;

        public TcpRequestMessage(IPAddress ip, string clientName) : base(ip)
        {
            this.clientName = clientName;
        }
    }
}
