using MessageClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Client
{
    public class PlayerData
    {
        private const int CLIENT_PORT = 0;
        private const int SERVER_PORT = 8000;
        private const int BUFF_SIZE = 2048;

        public IPEndPoint serverIpPort;
        public IPAddress clientIp;

        public Socket udpListenSocket;
        public Socket tcpListenSocket;

        public BinarySerializer serializer;

        public List<PublicMessage> ListOfPublicMessages;
        public List<PrivateMessage> ListOfPrivateMessages;
        public List<Participant> ListOfParticipants;

        public Thread tcpThread;
        public Thread udpThread;

        private readonly string redColor = "#df5154";
        private readonly string blueColor = "#3aade8";
        private readonly string greenColor = "#8bcb5a";
        private readonly string purpleColor = "#9400D3";

        public int CurrentFieldIndex;
        public int FinalFieldIndex;
        public Point CurrentPosition;
        public int Index;
        public string Name;
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

        public MainWindow mainWindow;

        public delegate void ListOfParticipantsReceived();
        public event ListOfParticipantsReceived ListOfParticipantsReceivedEvent;

        public delegate void StartGameMessageReceived();
        public event StartGameMessageReceived StartGameMessageReceivedEvent;

        public delegate void CubeMessageReceived();
        public event CubeMessageReceived CubeMessageReceivedEvent;

        public delegate void PublicMessageReceived(PublicMessage message);
        public event PublicMessageReceived PublicMessageReceivedEvent;

        public delegate void DescriptionMessageReceived(DescriptionMessage message);
        public event DescriptionMessageReceived DescriptionMessageReceivedEvent;

        public delegate void MoneyMessageReceived();
        public event MoneyMessageReceived MoneyMessageReceivedEvent;

        public delegate void MoveMessageReceived(MoveMessage message);
        public MoveMessageReceived MoveMessageReceivedEvent;

        public delegate void CasinoMessageReceived();
        public event CasinoMessageReceived CasinoMessageReceivedEvent;

        public delegate void DisconnectMessageReceived(DisconnectMessage message);
        public event DisconnectMessageReceived DisconnectMessageReceivedEvent;

        public PlayerData(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            serializer = new BinarySerializer();
            ListOfPublicMessages = new List<PublicMessage>();
            ListOfPrivateMessages = new List<PrivateMessage>();
            ListOfParticipants = new List<Participant>();
            udpListenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            tcpListenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            clientIp = myIPv4.GetIPv4();
            IPEndPoint clientIpPort = new IPEndPoint(clientIp, CLIENT_PORT);

            udpListenSocket.Bind(clientIpPort);
            tcpListenSocket.Bind(clientIpPort);

            udpThread = new Thread(UDPConnection);
            tcpThread = new Thread(TCPConnection);
        }

        public void UDPConnection()
        {
            try
            {
                int numberOfBytes = 0;
                byte[] data = new byte[BUFF_SIZE];

                EndPoint serverEndPoint = new IPEndPoint(IPAddress.Any, CLIENT_PORT);

                MemoryStream stream = new MemoryStream();
                using (stream)
                {
                    do
                    {
                        numberOfBytes = udpListenSocket.ReceiveFrom(data, ref serverEndPoint);
                        stream.Write(data, 0, numberOfBytes);
                    }
                    while (udpListenSocket.Available > 0);

                    if (numberOfBytes > 0)
                    {
                        IdentifyMessage(serializer.Deserialize(stream.ToArray()));
                    }
                }
            }
            catch
            {
                DisconnectClient();
            }
        }

        public void TCPConnection()
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
                            numberOfBytes = tcpListenSocket.Receive(data, data.Length, SocketFlags.None);
                            stream.Write(data, 0, numberOfBytes);
                        }
                        while (tcpListenSocket.Available > 0);

                        if (numberOfBytes > 0)
                        {
                            IdentifyMessage(serializer.Deserialize(stream.ToArray()));
                        }

                    }
                }
            }
            catch
            {
                DisconnectClient();
            }

        }

        public void SendUdpRequest()
        {
            IPEndPoint clientEndPoint = (IPEndPoint)udpListenSocket.LocalEndPoint;
            udpListenSocket.EnableBroadcast = true;

            UdpRequestMessage udpRequest = new UdpRequestMessage(clientEndPoint.Address, clientEndPoint.Port);
            udpListenSocket.SendTo(serializer.Serialize(udpRequest), new IPEndPoint(IPAddress.Broadcast, SERVER_PORT));

            udpThread.Start();
        }

        private void CloseSocket(ref Socket socket)
        {
            if (socket != null)
            {
                socket.Close();
                socket = null;
            }
        }

        private void CloseThread(ref Thread thread)
        {
            if (thread != null)
            {
                thread.Abort();
                thread = null;
            }
        }

        public void SendTcpRequest()
        {
            tcpListenSocket.Connect(serverIpPort);
            TcpRequestMessage request = new TcpRequestMessage(clientIp, Name, (int)CurrentPosition.X, (int)CurrentPosition.Y, CurrentFieldIndex, FinalFieldIndex, Money, Color, LeftCube, RightCube, movesLeft);
            tcpListenSocket.Send(serializer.Serialize(request));
            tcpThread.Start();
            CloseSocket(ref udpListenSocket);
            CloseThread(ref udpThread);
        }

        public void SendPublicMessage(string data)
        {
            var message = new PublicMessage(clientIp, DateTime.Now, Index, Name, data);
            tcpListenSocket.Send(serializer.Serialize(message));
        }

        public void SendDescriptionMessage(string data)
        {
            var message = new DescriptionMessage(clientIp, Index, Name, data);
            tcpListenSocket.Send(serializer.Serialize(message));
        }

        public void SendMoneyMessage(MoneyMessage message)
        {
            tcpListenSocket.Send(serializer.Serialize(message));
        }

        public void SendCubeMessage(CubeMessage message)
        {
            tcpListenSocket.Send(serializer.Serialize(message));
        }

        public void SendNextPlayerMoveMessage(Participant currentPlayer)
        {
            MoveMessage message = new MoveMessage(clientIp, currentPlayer.id, DateTime.Now);
            tcpListenSocket.Send(serializer.Serialize(message));
        }

        public void SendDisconnectMessage(DisconnectMessage disconnectMessage)
        {
            tcpListenSocket.Send(serializer.Serialize(disconnectMessage));
        }

        public void SendCasinoMessage(CasinoMessage message)
        {
            tcpListenSocket.Send(serializer.Serialize(message));
        }

        public void IdentifyMessage(Message message)
        {
            try
            {
                if (message is UdpRequestMessage) GetServerUdpRespond((UdpRequestMessage)message);
                if (message is PublicMessage) GetPublicMessage((PublicMessage)message);
                if (message is CubePanelMessage) GetStartGameMessage((CubePanelMessage)message);
                if (message is ClientIdMessage) GetClientIdMessage((ClientIdMessage)message);
                if (message is ListOfParticipantsMessage) GetListOfParticipantsMessage((ListOfParticipantsMessage)message);
                if (message is CubeMessage) GetCubesValue((CubeMessage)message);
                if (message is DescriptionMessage) GetMoveDescription((DescriptionMessage)message);
                if (message is MoneyMessage) GetMoneyMessage((MoneyMessage)message);
                if (message is MoveMessage) GetMoveMessage((MoveMessage)message);
                if (message is CasinoMessage) GetCasinoMessage((CasinoMessage)message);
                if (message is DisconnectMessage) GetDisconnectMessage((DisconnectMessage)message);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void GetDisconnectMessage(DisconnectMessage message)
        {
            DisconnectMessageReceivedEvent(message);
        }

        private void GetCasinoMessage(CasinoMessage message)
        {
            LeftCube = message.LeftCube;
            CasinoMessageReceivedEvent();
        }

        private void GetMoveMessage(MoveMessage message)
        {
            MoveMessageReceivedEvent(message);
        }

        private void GetMoneyMessage(MoneyMessage message)
        {
            ListOfParticipants = message.ListOfParticipants;
            MoneyMessageReceivedEvent();
        }

        private void GetMoveDescription(DescriptionMessage description)
        {
            DescriptionMessageReceivedEvent(description);
        }

        private void GetCubesValue(CubeMessage cube)
        {
            LeftCube = cube.LeftCube;
            RightCube = cube.RightCube;
            CubeMessageReceivedEvent();
        }

        private void GetServerUdpRespond(UdpRequestMessage server)
        {
            serverIpPort = new IPEndPoint(server.ip, server.port);
        }

        private void GetPublicMessage(PublicMessage message)
        {
            //ListOfPublicMessages.Add(message);
            PublicMessageReceivedEvent(message);
        }

        private void GetStartGameMessage(CubePanelMessage message)
        {
            if (Index == message.id) StartGameMessageReceivedEvent();
        }

        private void GetClientIdMessage(ClientIdMessage message)
        {
            Index = message.id;
        }

        private void GetListOfParticipantsMessage(ListOfParticipantsMessage message)
        {
            ListOfParticipants = message.ListOfParticipants;
            ListOfParticipantsReceivedEvent();

        }

        public void DisconnectClient()
        {
            CloseSocket(ref tcpListenSocket);
            CloseThread(ref tcpThread);
            CloseSocket(ref udpListenSocket);
            CloseThread(ref udpThread);
        }

        public string GetClientNameById(int id)
        {
            string name = "";
            foreach (var participant in ListOfParticipants)
            {
                if (participant.id == id)
                {
                    name = participant.name;
                    return name;
                }
            }
            return name;
        }
    }
}