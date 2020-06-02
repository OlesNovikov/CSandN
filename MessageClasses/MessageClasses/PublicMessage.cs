using System;
using System.Net;

namespace MessageClasses
{
    [Serializable]
    public class PublicMessage : Message
    {
        public int ID;
        public string data;
        public string senderName;
        public DateTime dateTime;

        public PublicMessage(IPAddress ip, DateTime dateTime, int ID, string senderName, string data) : base(ip)
        {
            this.ID = ID;
            this.data = data;
            this.senderName = senderName;
            this.dateTime = dateTime;
        }
    }
}
