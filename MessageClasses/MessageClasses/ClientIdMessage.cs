using System;
using System.Net;

namespace MessageClasses
{
    [Serializable]
    public class ClientIdMessage : Message
    {
        public int id;

        public ClientIdMessage(IPAddress ip, int id) : base(ip)
        {
            this.id = id;
        }
    }
}
