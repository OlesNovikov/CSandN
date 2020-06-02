using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MessageClasses
{
    [Serializable]
    public class CubePanelMessage : ClientIdMessage
    {
        public bool move;

        public CubePanelMessage(IPAddress ip, int id, bool move) : base(ip, id)
        {
            this.move = move;
        }
    }
}
