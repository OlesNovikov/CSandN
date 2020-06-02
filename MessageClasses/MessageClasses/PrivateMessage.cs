using System;
using System.Net;

namespace MessageClasses
{
    [Serializable]
    public class PrivateMessage : PublicMessage
    {
        public int receiverId;

        public PrivateMessage(IPAddress clientIp, DateTime dateTime, int senderId, string senderName, string data, int receiverId) : base(clientIp, dateTime, senderId, senderName, data)
        {
            this.receiverId = receiverId;
        }
    }
}
