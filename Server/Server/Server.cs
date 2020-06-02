using System.Text;
using System;
using System.Threading.Tasks;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.Collections.Generic;
using System.IO;
using MessageClasses;

namespace Server
{
    public class Server
    {
        const int SERVER_PORT = 8000;
        const int CLIENT_PORT = 0;
        const int BUFF_SIZE = 1024;
        const int maxConnections = 10;
        int currentMaxId = 0;

        Socket udpListenSocket;
        Socket tcpListenSocket;
        Thread listenUdpThread;
        Thread listenTcpThread;

        IPAddress serverIp;
        IPEndPoint UdpListenIpPort;
        IPEndPoint TcpListenIpPort;
        IPEndPoint clientIpPort;
        BinarySerializer serializer;

        List<Client> ListOfClients;
        List<PublicMessage> ListOfPublicMessages;

        // method takes the IPv4 address from list of addresses of this host
        private IPAddress GetServerIpv4()
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

        // creates empty list of messages and clients
        private void CreateMessagesStorage()
        {
            serializer = new BinarySerializer();
            ListOfClients = new List<Client>();
            ListOfPublicMessages = new List<PublicMessage>();
        }

        // creates sockets, that will listen for UDP, TCP requests
        public Server()
        {
            udpListenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            tcpListenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverIp = GetServerIpv4();

            CreateMessagesStorage();
        }

        // initializes IPEndPoints for sockets to listen
        public void StartListen()
        {
            UdpListenIpPort = new IPEndPoint(IPAddress.Any, SERVER_PORT);
            TcpListenIpPort = new IPEndPoint(serverIp, SERVER_PORT);

            udpListenSocket.Bind(UdpListenIpPort);
            tcpListenSocket.Bind(TcpListenIpPort);

            Console.WriteLine("Server start listen UDP requests...");
            Console.WriteLine("Server start listen TCP requests...");

            CreateListenThreads();
        }

        // deserializes received message
        private Message DeserializedMessage(MemoryStream stream)
        {
            Message message = serializer.Deserialize(stream.ToArray());
            return message;
        }

        // start listen threads
        private void CreateListenThreads()
        {
            listenUdpThread = new Thread(ListenUdp);
            listenTcpThread = new Thread(ListenTcp);

            listenUdpThread.Start();
            listenTcpThread.Start();
        }

        // from this point server knows clients ip and port
        private void ListenUdp()
        {
            EndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, CLIENT_PORT);

            int numberOfBytes = 0;

            try
            {
                while (true)
                {
                    byte[] data = new byte[BUFF_SIZE];

                    MemoryStream stream = new MemoryStream();
                    using (stream)
                    {
                        do
                        {
                            numberOfBytes = udpListenSocket.ReceiveFrom(data, ref clientEndPoint);
                            stream.Write(data, 0, numberOfBytes);
                        }
                        while (udpListenSocket.Available > 0);

                        if (numberOfBytes > 0)
                        {
                            clientIpPort = clientEndPoint as IPEndPoint;
                            Console.WriteLine("**" + DateTime.Now.ToShortTimeString() + "**" + " UDP from " + clientIpPort.Port.ToString() + " ip: " + clientIpPort.Address.ToString());

                            IdentifyMessage(DeserializedMessage(stream));
                        }
                        else
                        {
                            Console.WriteLine("**" + DateTime.Now.ToShortTimeString() + "**" + " UDP from " + clientIpPort.Port.ToString() + " ip: " + clientIpPort.Address.ToString() + " is empty");
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
        }

        private void ListenTcp()
        {
            tcpListenSocket.Listen(maxConnections);

            int numberOfBytes = 0;

            try
            {
                while (true)
                {
                    Socket tcpHandler = tcpListenSocket.Accept();

                    byte[] data = new byte[BUFF_SIZE];
                    MemoryStream stream = new MemoryStream();

                    using (stream)
                    {
                        do
                        {
                            numberOfBytes = tcpHandler.Receive(data);
                            stream.Write(data, 0, numberOfBytes);
                        }
                        while (tcpListenSocket.Available > 0);

                        if (numberOfBytes > 0)
                        {
                            Console.WriteLine("**" + DateTime.Now.ToShortTimeString() + "**" + " TCP from " + clientIpPort.Port.ToString() + " ip: " + clientIpPort.Address.ToString());
                            AddNewClient(DeserializedMessage(stream), tcpHandler);
                        }
                        else
                        {
                            Console.WriteLine("**" + DateTime.Now.ToShortTimeString() + "**" + " TCP from " + clientIpPort.Port.ToString() + " ip: " + clientIpPort.Address.ToString() + " is empty");
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
        }

        public void SendIdMessage(TcpRequestMessage request, Client client)
        {
            Message idMessage = new ClientIdMessage(request.ip, client.id);
            client.tcpHandler.Send(serializer.Serialize(idMessage));
        }   

        public void SendListOfParticipants()
        {
            List<Participant> ListOfParticipants = GetParticipants();
            ListOfParticipantsMessage membersListMessage = new ListOfParticipantsMessage(serverIp, ListOfParticipants);

            foreach (var clientInList in ListOfClients)
            {
                clientInList.tcpHandler.Send(serializer.Serialize(membersListMessage));
            }
        }

        public void SendPublicConnectedMessage(TcpRequestMessage connectionRequest, Client client)
        {
            Message publicMessage = new PublicMessage(connectionRequest.ip, DateTime.Now, client.id, client.Name, client.Name + " connected to game");
            foreach (var clientt in ListOfClients)
            {
                clientt.tcpHandler.Send(serializer.Serialize(publicMessage));
            }
        }

        public void AddNewClient(Message message, Socket tcpHandler)
        {
            try
            {
                if (message is TcpRequestMessage)
                {
                    TcpRequestMessage request = (TcpRequestMessage)message;
                    Client client = new Client(serializer, request.ip, request.clientName, currentMaxId, request.CurrentPositionX, request.CurrentPositionY, tcpHandler, request.CurrentFieldIndex, request.FinalFieldIndex, request.Money, currentMaxId.ToString(), request.LeftCube, request.RightCube, request.movesLeft);

                    client.messageIdentification += IdentifyMessage;
                    client.ClientDisconnectedEvent += DisconnectClient;
                    ListOfClients.Add(client);
                    currentMaxId++;

                    Console.WriteLine("**" + DateTime.Now.ToShortTimeString() + "**" + " id = " + client.id.ToString() + " " + client.Name + " connected to game");

                    SendIdMessage(request, client);
                    
                    SendListOfParticipants();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
        }

        public void SendMoveMessage(CubePanelMessage message)
        {
            foreach (var client in ListOfClients)
            {
                if (client.id == message.id) client.tcpHandler.Send(serializer.Serialize(message));
            }
        }

        public void DisconnectClient(Client disconnectedClient)
        {
            if (ListOfClients.Remove(disconnectedClient))
            {
                Console.WriteLine(DateTime.Now.ToShortTimeString() + " " + disconnectedClient.Name + " left the game");
            }
        }

        public List<Participant> GetParticipants()
        {
            List<Participant> participantsList = new List<Participant>();
            foreach (var client in ListOfClients)
            {
                participantsList.Add(new Participant(client.Name, client.id, client.CurrentPositionX, client.CurrentPositionX, client.CurrentFieldIndex, client.FinalFieldIndex, client.Money, client.Color, client.LeftCube, client.RightCube, client.movesLeft));
            }
            return participantsList;
        }

        // identifies what type of message received
        public void IdentifyMessage(Message message)
        {
            try
            {
                if (message is UdpRequestMessage) SendUdpRespond((UdpRequestMessage)message);
                if (message is PublicMessage) SendPublicMessage((PublicMessage)message);
                if (message is PrivateMessage) AddPrivateMessage((PrivateMessage)message);
                if (message is HistoryRequestMessage) SendHistory((HistoryRequestMessage)message);
                if (message is CubeMessage) SendCubesValue((CubeMessage)message);
                if (message is DescriptionMessage) SendMoveDescription((DescriptionMessage)message);
                if (message is MoneyMessage) SendMoneyMessage((MoneyMessage)message);
                if (message is MoveMessage) SendMoveMessage((MoveMessage)message);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
        }

        public void SendMoveMessage(MoveMessage message)
        {
            foreach (var client in ListOfClients)
            {
                client.tcpHandler.Send(serializer.Serialize(message));
            }
        }

        public void SendMoneyMessage(MoneyMessage message)
        {
            foreach (var player in ListOfClients)
            {
                player.tcpHandler.Send(serializer.Serialize(message));
                Console.WriteLine("Update MoneyMessage to " + player.Name);
            }
        }

        public void SendMoveDescription(DescriptionMessage description)
        {
            foreach (var player in ListOfClients)
            {
                player.tcpHandler.Send(serializer.Serialize(description));
                Console.WriteLine("DescriptionMessage {" + description.data + "} to " + player.Name);
            }
        }

        public void SendCubesValue(CubeMessage message)
        {
            Random number = new Random();
            message.LeftCube = number.Next(1, 7);
            message.RightCube = number.Next(1, 7);
            foreach (var player in ListOfClients)
            {
                player.tcpHandler.Send(serializer.Serialize(message));  
            }
        }

        // sends server ip + port to client
        private void SendUdpRespond(UdpRequestMessage client)
        {
            IPEndPoint clientIpPort = new IPEndPoint(client.ip, client.port);
            Message message = new UdpRequestMessage(serverIp, SERVER_PORT);

            udpListenSocket.SendTo(serializer.Serialize(message), clientIpPort);
        }

        // stores received public message
        private void AddPublicMessage(PublicMessage message)
        {
            ListOfPublicMessages.Add(message);
            SendPublicMessage(message);
        }

        // send message to all connected clients
        public void SendPublicMessage(PublicMessage message)
        {
            foreach (var client in ListOfClients)
            {
                client.tcpHandler.Send(serializer.Serialize(message));
            }
        }

        // sends private message to client, chosen by ID field in PrivateMessage class
        private void AddPrivateMessage(PrivateMessage message)
        {
            Client receiver = GetReceiverById(message);
            receiver.tcpHandler.Send(serializer.Serialize(message));
        }

        // returns client by ID field in PrivateMessage class
        private Client GetReceiverById(PrivateMessage message)
        {
            Client client = null;
            foreach (var clientInList in ListOfClients)
            {
                if (clientInList.id == message.receiverId) return clientInList;
            }
            return client;
        }

        private void SendHistory(HistoryRequestMessage message)
        {
            foreach (var client in ListOfClients)
            {
                if (client.id == message.id)
                {
                    client.tcpHandler.Send(serializer.Serialize(new HistoryRespondMessage(serverIp, ListOfPublicMessages)));
                }
            }
        }

        ~Server()
        {
            CloseServer();
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

        public void CloseServer()
        {
            foreach (var client in ListOfClients)
            {
                CloseSocket(ref client.tcpHandler);
                CloseThread(ref client.listenTCP);
            }
            CloseSocket(ref udpListenSocket);
            CloseSocket(ref tcpListenSocket);
            CloseThread(ref listenUdpThread);
            CloseThread(ref listenTcpThread);
        }

    }
}
