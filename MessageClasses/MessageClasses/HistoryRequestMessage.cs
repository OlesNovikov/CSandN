using System;
using System.Net;

namespace MessageClasses
{
    [Serializable]
    public class HistoryRequestMessage : Message
    {
        public int id;

        public HistoryRequestMessage(IPAddress ip, int id) : base(ip)
        {
            this.id = id;
        }
    }
}
