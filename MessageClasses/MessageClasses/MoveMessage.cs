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
        public DateTime dateTime;
        public int id;

        public MoveMessage(IPAddress ip, int id, DateTime dateTime) : base (ip)
        {
            this.dateTime = dateTime;
            this.id = id;
        }
    }
}
