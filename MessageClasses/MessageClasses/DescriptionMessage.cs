using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MessageClasses
{
    [Serializable]
    public class DescriptionMessage : Message
    {
        public int ID;
        public string senderName;
        public string data;

        public DescriptionMessage(IPAddress ip, int ID, string senderName, string data) : base(ip)
        {
            this.ID = ID;
            this.senderName = senderName;
            this.data = data;
        }
    }
}
