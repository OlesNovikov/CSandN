using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MessageClasses
{
    [Serializable]
    public class HistoryResponseMessage : Message
    {
        public Dictionary<int, int> DictionaryOfSizes;
        public List<PublicMessage> ListOfMessages;
        public List<string> ListOfNames;

        public HistoryResponseMessage(IPAddress ip, List<PublicMessage> ListOfMessages, List<string> ListOfNames, Dictionary<int, int> DictionaryOfSizes) : base(ip)
        {
            this.ListOfMessages = ListOfMessages;
            this.ListOfNames = ListOfNames;
            this.DictionaryOfSizes = DictionaryOfSizes;
        }
    }
}
