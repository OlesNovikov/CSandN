using System;
using System.Collections.Generic;
using System.Net;

namespace MessageClasses
{
    [Serializable]
    public class PrivateMessage : Message
    {
        public DateTime dateTime;
        public int receiverId;
        public int senderId;
        public string data;
        public Dictionary<int, string> DictionaryOfFiles;
        public Dictionary<int, int> DictionaryOfSizes;

        public PrivateMessage(IPAddress ip, DateTime dateTime, int senderId, string data, int receiverId, Dictionary<int, string> DictionaryOfFiles, Dictionary<int, int> DictionaryOfSizes) : base(ip)
        {
            this.dateTime = dateTime;
            this.senderId = senderId;
            this.receiverId = receiverId;
            this.data = data;
            this.DictionaryOfFiles = DictionaryOfFiles;
            this.DictionaryOfSizes = DictionaryOfSizes;
        }
    }
}
