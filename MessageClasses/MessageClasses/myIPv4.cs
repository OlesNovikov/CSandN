using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MessageClasses
{
    public static class myIPv4
    {
        public static IPAddress GetIPv4()
        {
            IPAddress[] allLocalIp = Dns.GetHostAddresses(Dns.GetHostName());
            IPAddress Ipv4 = null;

            foreach (var ip in allLocalIp)
            {
                if (ip.GetAddressBytes().Length == 4)
                {
                    Ipv4 = ip;
                    return Ipv4;
                }
            }
            return Ipv4;
        }
    }
}
