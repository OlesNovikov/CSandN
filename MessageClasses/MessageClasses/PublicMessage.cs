using System;
using System.Collections.Generic;
using System.Net;

namespace MessageClasses
{
    [Serializable]
    public class PublicMessage : Message
    {
        public DateTime dateTime;
        public Dictionary<int, string> DictionaryOfFiles;
        public Dictionary<int, int> DictionaryOfSizes;

        public int ID;
        public string data;

        public PublicMessage(IPAddress ip, DateTime dateTime, int ID, string data, Dictionary<int, string> DictionaryOfFiles, Dictionary<int, int> DictionaryOfSizes) : base(ip)
        {
            this.dateTime = dateTime;
            this.ID = ID;
            this.data = data;
            this.DictionaryOfFiles = DictionaryOfFiles;
            this.DictionaryOfSizes = DictionaryOfSizes;
        }
    }
}
