using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MessageClasses
{
    [Serializable]
    public class CasinoMessage : Message
    {
        public int LeftCube;

        public CasinoMessage(IPAddress ip, int LeftCube) : base(ip)
        {
            this.LeftCube = LeftCube;
        }
    }
}
