using System;
using System.Net;

namespace MessageClasses
{
    [Serializable]
    public class ConnectionRequest : Message
    {
        public string clientName;

        public ConnectionRequest(IPAddress ip, string clientName) : base(ip)
        {
            this.clientName = clientName;
        }
    }
}
