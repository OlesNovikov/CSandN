using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MessageClasses
{
    [Serializable]
    public class CubeMessage : Message
    {
        public int LeftCube;
        public int RightCube;
        public int id;

        public CubeMessage(IPAddress ip, int id, int LeftCube, int RightCube) : base(ip)
        {
            this.id = id;
            this.LeftCube = LeftCube;
            this.RightCube = RightCube;
        }
    }
}
