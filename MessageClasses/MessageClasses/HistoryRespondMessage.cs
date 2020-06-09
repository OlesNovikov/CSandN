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
        public List<string> ListOfNames;

        public HistoryRespondMessage(IPAddress ip, List<PublicMessage> ListOfMessages, List<string> ListOfNames) : base(ip)
        {
            this.ListOfMessages = ListOfMessages;
            this.ListOfNames = ListOfNames;
        }
    }
}
