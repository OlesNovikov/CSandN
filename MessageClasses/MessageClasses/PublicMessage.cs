using System;
using System.Net;

namespace MessageClasses
{
    [Serializable]
    public class PublicMessage : Message
    {
        public DateTime dateTime;

        public int ID;
        public string data;

        public PublicMessage(IPAddress ip, DateTime dateTime, int ID, string data) : base(ip)
        {
            this.dateTime = dateTime;
            this.ID = ID;
            this.data = data;
        }
    }
}
