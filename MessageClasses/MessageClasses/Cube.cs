using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class Cube
    {
        public int LeftCube;
        public int RightCube;

        public int SetValueToCube(Random value)
        {
            int RandomNumber = value.Next(1, 7);
            return RandomNumber;
        }
    }
}
