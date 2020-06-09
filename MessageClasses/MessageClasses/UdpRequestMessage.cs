using System;
using System.Net;

namespace MessageClasses
{
    [Serializable]
    public class UdpRequestMessage : Message
    {
        public int port;

        public UdpRequestMessage(IPAddress ip, int port) : base(ip)
        {
            this.port = port;
        }
    }
}
