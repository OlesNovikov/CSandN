using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MessageClasses;

namespace Server
{
    public class Client
    {
        const int BUFF_SIZE = 1024;

        private readonly string redColor = "#df5154";
        private readonly string blueColor = "#3aade8";
        private readonly string greenColor = "#8bcb5a";
        private readonly string purpleColor = "#9400D3";

        public string Name;
        public IPAddress ip;
        public int id;
        public int CurrentPositionX;
        public int CurrentPositionY;
        public int CurrentFieldIndex;
        public int FinalFieldIndex;
        public int Money;
        private string _color;
        public string Color
        {
            get
            {
                return _color;
            }
            set
            {
                if (value == "0") _color = redColor;
                else if (value == "1") _color = greenColor;
                else if (value == "2") _color = blueColor;
                else _color = purpleColor;
            }
        }
        public int LeftCube;
        public int RightCube;
        public int movesLeft;

        public Socket tcpHandler;
        public Thread listenTCP;

        public BinarySerializer serializer;

        public delegate void IdentifyMessage(Message message);
        public event IdentifyMessage messageIdentification;

        public delegate void ClientDisconnected(Client client);
        public event ClientDisconnected ClientDisconnectedEvent;

        public Client(BinarySerializer serializer, IPAddress ip, string Name, int id, int CurrentPositionX, int CurrentPositionY, Socket tcpHandler, int CurrentFieldIndex, int FinalFieldIndex, int Money, string Color, int LeftCube, int RightCube, int movesLeft)
        {
            this.serializer = serializer;
            this.ip = ip;
            this.Name = Name;
            this.id = id;
            this.CurrentPositionX = CurrentPositionX;
            this.CurrentPositionY = CurrentPositionY;
            this.tcpHandler = tcpHandler;
            this.CurrentFieldIndex = CurrentFieldIndex;
            this.FinalFieldIndex = FinalFieldIndex;
            this.Money = Money;
            this.Color = Color;
            this.LeftCube = LeftCube;
            this.RightCube = RightCube;
            this.movesLeft = movesLeft;

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
                            messageIdentification(serializer.Deserialize(stream.ToArray()));
                        }
                    }
                }
            }
            catch (SocketException)
            {
                ClientDisconnectedEvent(this);
                CloseSocket(ref tcpHandler);
                CloseThread(ref listenTCP);
            }
        }

        public void CloseSocket(ref Socket socket)
        {
            if (socket != null)
            {
                socket.Close();
                socket = null;
            }
        }

        public void CloseThread(ref Thread thread)
        {
            if (thread != null)
            {
                thread.Abort();
                thread = null;
            }
        }
    }
}
