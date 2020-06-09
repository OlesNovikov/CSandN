using System;
using System.Net;

namespace MessageClasses
{
    [Serializable]
    public class PrivateMessage : Message
    {
        public DateTime dateTime;
        public int receiverId;
        public int senderId;
        public string data;

        public PrivateMessage(IPAddress ip, DateTime dateTime, int senderId, string data, int receiverId) : base(ip)
        {
            this.dateTime = dateTime;
            this.senderId = senderId;
            this.receiverId = receiverId;
            this.data = data;
        }
    }
}
