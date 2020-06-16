using System;
using System.Net;

namespace MessageClasses
{
    [Serializable]
    public class BroadcastMessage : Message
    {
        public int port;

        public BroadcastMessage(IPAddress ip, int port) : base(ip)
        {
            this.port = port;
        }
    }
}
