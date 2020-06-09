using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.Windows;
using MessageClasses;
using System.IO;

namespace ChatClient
{
    [Serializable]
    public class Client
    {
        private const int CLIENT_PORT = 0;
        private const int SERVER_PORT = 8000;
        private const int BUFF_SIZE = 1024;

        public string Name;
        public int id;

        public IPEndPoint serverIpPort;
        public IPAddress ip;

        private Socket udpListenSocket;
        private Socket tcpListenSocket;

        public BinarySerializer serializer;

        public List<PublicMessage> ListOfPublicMessages;
        public List<PrivateMessage> ListOfPrivateMessages;
        public List<Participant> ListOfParticipants;
        public List<string> ListOfNames;

        public delegate void MessageReceived(Message message);
        public event MessageReceived MessageReceivedEvent;

        public delegate void ListOfParticipantsReceived();
        public event ListOfParticipantsReceived ListOfParticipantsReceivedEvent;

        public delegate void HistoryMessageReceived();
        public event HistoryMessageReceived HistoryMessageReceivedEvent;

        public Thread tcpThread;
        public Thread udpThread;

        public Client()
        {
            serializer = new BinarySerializer();
            ListOfPublicMessages = new List<PublicMessage>();
            ListOfParticipants = new List<Participant>();
            ListOfPrivateMessages = new List<PrivateMessage>();
            ListOfNames = new List<string>();
            udpListenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            tcpListenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            ip = GetClientIpv4();
            IPEndPoint clientIpPort = new IPEndPoint(ip, CLIENT_PORT);

            udpListenSocket.Bind(clientIpPort);
            tcpListenSocket.Bind(clientIpPort);

            udpThread = new Thread(UDPConnection);
            tcpThread = new Thread(TCPConnection);
        }

        public IPAddress GetClientIpv4()
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

        public void SendUdpRequest()
        {
            IPEndPoint clientEndPoint = (IPEndPoint)udpListenSocket.LocalEndPoint;
            udpListenSocket.EnableBroadcast = true;

            UdpRequestMessage udpRequest = new UdpRequestMessage(clientEndPoint.Address, clientEndPoint.Port);
            udpListenSocket.SendTo(serializer.Serialize(udpRequest), new IPEndPoint(IPAddress.Broadcast, SERVER_PORT));

            udpThread.Start();
        }

        public void UDPConnection()
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

        public void SendTcpRequest()
        {
            tcpListenSocket.Connect(serverIpPort);
            TcpRequestMessage request = new TcpRequestMessage(ip, Name);
            tcpListenSocket.Send(serializer.Serialize(request));
            tcpThread.Start();
            CloseSocket(ref udpListenSocket);
            CloseThread(ref udpThread);
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
                            numberOfBytes = tcpListenSocket.Receive(data, data.Length, 0);
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

        public void SendPublicMessage(string data)
        {
            Message message = new PublicMessage(ip, DateTime.Now, id, data);
            tcpListenSocket.Send(serializer.Serialize(message));
        }

        public void SendPrivateMessage(PrivateMessage message)
        {
            tcpListenSocket.Send(serializer.Serialize(message));
        }

        public void SendHistoryRequest(HistoryRequestMessage request)
        {
            tcpListenSocket.Send(serializer.Serialize(request));
        }

        public void IdentifyMessage(Message message)
        {
            try
            {
                if (message is UdpRequestMessage) GetServerUdpRespond((UdpRequestMessage)message);
                if (message is PublicMessage)
                {
                    GetPublicMessage((PublicMessage)message);
                }
                if (message is PrivateMessage)
                {
                    GetPrivateMessage((PrivateMessage)message);
                }
                if (message is HistoryRespondMessage)
                {
                    GetHistoryRequestMessage((HistoryRespondMessage)message);
                } 
                if (message is ClientIdMessage) GetClientIdMessage((ClientIdMessage)message);
                if (message is ListOfParticipantsMessage)
                {
                    GetListOfParticipantsMessage((ListOfParticipantsMessage)message);
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void GetServerUdpRespond(UdpRequestMessage server)
        {
            serverIpPort = new IPEndPoint(server.ip, server.port);
        }

        private void GetPublicMessage(PublicMessage message)
        {
            ListOfPublicMessages.Add(message);
            MessageReceivedEvent(message);
        }

        private void GetPrivateMessage(PrivateMessage message)
        {
            if ((id == message.receiverId) || (id == message.senderId)) ListOfPrivateMessages.Add(message);
            MessageReceivedEvent(message);
        }

        private void GetHistoryRequestMessage(HistoryRespondMessage message)
        {
            ListOfNames = message.ListOfNames;
            ListOfPublicMessages = message.ListOfMessages;
            HistoryMessageReceivedEvent();
        }

        private void GetClientIdMessage(ClientIdMessage message)
        {
            id = message.id;
        }

        private void GetListOfParticipantsMessage(ListOfParticipantsMessage message)
        {
            ListOfParticipants = message.ListOfParticipants;
            ListOfParticipantsReceivedEvent();
        }

        public string GetClientNameById(int id)
        {
            foreach (var participant in ListOfParticipants)
            {
                if (participant.id == id)
                {
                    return participant.name;
                }
            }
            return "";
        }

        public string GetNameFromList(int id)
        {
            return ListOfNames[id];
        }

        public void DisconnectClient()
        {
            CloseSocket(ref tcpListenSocket);
            CloseThread(ref tcpThread);
            CloseSocket(ref udpListenSocket);
            CloseThread(ref udpThread);
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
    }
}
