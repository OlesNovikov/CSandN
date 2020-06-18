using FileServiceLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Program
    {
        const string PREFIX = "http://localhost:8080/";

        static void Main(string[] args)
        {
            Server server = new Server();
            FileServer fServer = new FileServer(PREFIX);

            server.StartListen();
        }
    }
}
