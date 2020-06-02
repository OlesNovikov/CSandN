using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MessageClasses
{
    [Serializable]
    public class HistoryRespondMessage : Message
    {
        public List<PublicMessage> ListOfMessages;

        public HistoryRespondMessage(IPAddress ip, List<PublicMessage> ListOfMessages) : base(ip)
        {
            this.ListOfMessages = ListOfMessages;
        }
    }
}
