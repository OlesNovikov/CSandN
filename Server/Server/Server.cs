using System.Text;
using System;
using System.Threading.Tasks;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.Collections.Generic;
using System.IO;
using MessageClasses;
using AdditionalLibrary;

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
        List<string> ListOfNames;

        // creates empty list of messages and clients
        private void CreateMessagesStorage()
        {
            serializer = new BinarySerializer();
            ListOfClients = new List<Client>();
            ListOfPublicMessages = new List<PublicMessage>();
            ListOfNames = new List<string>();
        }

        // creates sockets, that will listen for UDP, TCP requests
        public Server()
        {
            udpListenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            tcpListenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverIp = MyIPv4.GetIPv4();

            Console.WriteLine("Server IP: " + serverIp.ToString());

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
                            Console.WriteLine(DateTime.Now.ToShortTimeString() + " UDP from " + clientIpPort.Port.ToString() + " ip: " + clientIpPort.Address.ToString());

                            IdentifyMessage(DeserializedMessage(stream));
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
                            Console.WriteLine(DateTime.Now.ToShortTimeString() + " TCP from " + clientIpPort.Port.ToString() + " ip: " + clientIpPort.Address.ToString());
                            AddNewClient(DeserializedMessage(stream), tcpHandler);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
        }

        public void SendMessageToAll(Message message)
        {
            foreach (var clientInList in ListOfClients)
            {
                clientInList.tcpHandler.Send(serializer.Serialize(message));
            }
        }

        public void SendIdMessage(TcpRequestMessage connectionRequest, Client client)
        {
            Message idMessage = new ClientIdMessage(connectionRequest.ip, client.id);
            client.tcpHandler.Send(serializer.Serialize(idMessage));
        }

        public void SendListOfParticipants()
        {
            List<Participant> ListOfParticipants = GetParticipants();
            ListOfParticipantsMessage membersListMessage = new ListOfParticipantsMessage(serverIp, ListOfParticipants);
            SendMessageToAll(membersListMessage);
        }

        public void SendPublicConnectedMessage(TcpRequestMessage connectionRequest, Client client)
        {
            var publicMessage = new PublicMessage(connectionRequest.ip, DateTime.Now, -1, client.Name + " connected to chat");
            ListOfPublicMessages.Add(publicMessage);
            SendMessageToAll(publicMessage);
        }

        public void AddNewClient(Message message, Socket tcpHandler)
        {
            try
            {
                if (message is TcpRequestMessage)
                {
                    TcpRequestMessage connectionRequest = (TcpRequestMessage)message;
                    Client client = new Client(connectionRequest.clientName, currentMaxId, tcpHandler);

                    client.MessageIdentification += IdentifyMessage;
                    client.ClientDisconnectedEvent += ClientDisconnected;
                    ListOfClients.Add(client);
                    ListOfNames.Add(client.Name);
                    currentMaxId++;

                    Console.WriteLine(DateTime.Now.ToShortTimeString() + " " + client.Name + "[" + client.id.ToString() + "]" + " connected to chat");

                    SendIdMessage(connectionRequest, client);
                    
                    SendListOfParticipants();

                    SendPublicConnectedMessage(connectionRequest, client);

                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
        }

        public void ClientDisconnected(Client client)
        {
            ListOfClients.Remove(client);
            Console.WriteLine(DateTime.Now.ToShortTimeString() + " " + client.Name + "[" + client.id.ToString() + "]" + " left chat");
            PublicMessage message = new PublicMessage(serverIp, DateTime.Now, -1, client.Name.ToString() + " left this chat");
            ListOfPublicMessages.Add(message);
            SendMessageToAll(message);
            SendListOfParticipants();
        }

        public List<Participant> GetParticipants()
        {
            List<Participant> participantsList = new List<Participant>();
            foreach (var client in ListOfClients)
            {
                participantsList.Add(new Participant(client.Name, client.id));
            }
            return participantsList;
        }

        public void IdentifyMessage(Message message)
        {
            try
            {
                if (message is UdpRequestMessage) SendUdpServerRespond((UdpRequestMessage)message);
                if (message is PublicMessage) AddPublicMessage((PublicMessage)message);
                if (message is PrivateMessage) AddPrivateMessage((PrivateMessage)message);
                if (message is HistoryRequestMessage) SendHistory((HistoryRequestMessage)message);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
        }

        private void SendUdpServerRespond(UdpRequestMessage client)
        {
            IPEndPoint clientIpPort = new IPEndPoint(client.ip, client.port);
            Message message = new UdpRequestMessage(serverIp, SERVER_PORT);

            udpListenSocket.SendTo(serializer.Serialize(message), clientIpPort);
        }

        private void AddPublicMessage(PublicMessage message)
        {
            string sender = "";
            foreach (var client in ListOfClients)
            {
                if (message.ID == client.id)
                {
                    sender = client.Name;
                } 
            }
            Console.WriteLine(DateTime.Now.ToShortTimeString() + " " + message.GetType().Name.ToString() + " from " + sender);
            ListOfPublicMessages.Add(message);
            SendMessageToAll(message);
        }

        private void AddPrivateMessage(PrivateMessage message)
        {
            Client receiver = GetClientById(message);
            receiver.tcpHandler.Send(serializer.Serialize(message));
            foreach (var client in ListOfClients)
            {
                if (client.id == message.senderId)
                {
                    Console.WriteLine(DateTime.Now.ToShortTimeString() + " " + message.GetType().Name.ToString() + " from " + client.Name + " to " + receiver.Name);
                    if (message.senderId != message.receiverId) client.tcpHandler.Send(serializer.Serialize(message));
                }
            }
        }

        private Client GetClientById(PrivateMessage message)
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
                    client.tcpHandler.Send(serializer.Serialize(new HistoryRespondMessage(serverIp, ListOfPublicMessages, ListOfNames)));
                }
            }
        }

        public void CloseServer()
        {
            foreach (var client in ListOfClients)
            {
                Close.CloseSocket(ref client.tcpHandler);
                Close.CloseThread(ref client.listenTCP);
            }
            Close.CloseSocket(ref udpListenSocket);
            Close.CloseSocket(ref tcpListenSocket);
            Close.CloseThread(ref listenUdpThread);
            Close.CloseThread(ref listenTcpThread);
        }

        ~Server()
        {
            CloseServer();
        }
    }
}
