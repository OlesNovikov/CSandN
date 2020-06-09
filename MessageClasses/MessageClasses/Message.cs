using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MessageClasses
{
    [Serializable]
    public class Message
    {
        public IPAddress ip;

        public Message(IPAddress ip)
        {
            this.ip = ip;
        }
    }
}
