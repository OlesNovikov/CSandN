using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MessageClasses
{
    [Serializable]
    public class MoveMessage : Message
    {
        public Participant currentPlayer;

        public MoveMessage(IPAddress ip, Participant currentPlayer) : base (ip)
        {
            this.currentPlayer = currentPlayer;
        }
    }
}
