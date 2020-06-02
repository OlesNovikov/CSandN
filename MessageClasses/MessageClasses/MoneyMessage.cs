using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MessageClasses
{
    [Serializable]
    public class MoneyMessage : Message
    {
        public List<Participant> ListOfParticipants;
        public DateTime time;

        public MoneyMessage(IPAddress ip, List<Participant> ListOfParticipants, DateTime time) : base(ip)
        {
            this.ListOfParticipants = ListOfParticipants;
            this.time = time;
        }
    }
}
