using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AdditionalLibrary;
using MessageClasses;

namespace Server
{
    public class Client
    {
        const int BUFF_SIZE = 1024;

        public string Name;
        public int id;
        public Socket tcpHandler;
        public Thread listenTCP;

        public BinarySerializer serializer;

        public delegate void IdentifyMessage(Message message);
        public event IdentifyMessage MessageIdentification;

        public delegate void ClientDisconnected(Client client);
        public event ClientDisconnected ClientDisconnectedEvent;

        public Client(string Name, int id, Socket tcpHandler)
        {
            this.Name = Name;
            this.id = id;
            this.tcpHandler = tcpHandler;

            serializer = new BinarySerializer();
            listenTCP = new Thread(ListenTCP);
            listenTCP.Start();
        }

        public void ListenTCP()
        {
            try
            {
                int numberOfBytes = 0;
                while (true)
                { 
                    byte[] data = new byte[BUFF_SIZE];
                    MemoryStream stream = new MemoryStream();

                    using (stream)
                    {
                        do
                        {
                            numberOfBytes = tcpHandler.Receive(data);
                            stream.Write(data, 0, numberOfBytes);
                        }
                        while (tcpHandler.Available > 0);

                        if (numberOfBytes > 0)
                        {
                            MessageIdentification(serializer.Deserialize(stream.ToArray()));
                        }
                    }
                }
            }
            catch
            {
                ClientDisconnectedEvent(this);
                Close.CloseSocket(ref tcpHandler);
                Close.CloseThread(ref listenTCP);
            }

        }
    }
}
