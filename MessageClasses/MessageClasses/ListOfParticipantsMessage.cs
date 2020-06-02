using System;
using System.Collections.Generic;
using System.Net;

namespace MessageClasses
{
    [Serializable]
    public class ListOfParticipantsMessage : Message
    {
        public List<Participant> ListOfParticipants;

        public ListOfParticipantsMessage(IPAddress ip, List<Participant> ListOfParticipants) : base(ip)
        {
            this.ListOfParticipants = ListOfParticipants;
        }
    }
}
