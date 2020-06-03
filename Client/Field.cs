using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Client
{
    public class Field
    {
        public string Name { get; set; }
        public int Price { get; set; }
        public int Deposit { get; set; }
        public int Buyout { get; set; }
        public int Star1 { get; set; }
        public int Star2 { get; set; }
        public int Star3 { get; set; }
        public int Star4 { get; set; }
        public int BigStar { get; set; }
        public int CurrentRent { get; set; }
        public int StarPrice { get; set; }
        public string OwnerColor { get; set; }
    }
}
