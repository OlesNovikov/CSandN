using MessageClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient
{
    public class BinarySerializer
    {
        BinaryFormatter formatter;

        public BinarySerializer()
        {
            formatter = new BinaryFormatter();
        }

        public byte[] Serialize(Message message)
        {
            MemoryStream stream = new MemoryStream();
            using (stream)
            {
                formatter.Serialize(stream, message);
            }
            return stream.ToArray();
        }

        public Message Deserialize(byte[] data)
        {
            Message message;
            MemoryStream stream = new MemoryStream();
            using (stream)
            {
                stream.Write(data, 0, data.Length);
                stream.Seek(0, SeekOrigin.Begin);
                message = (Message)formatter.Deserialize(stream);
            }
            return message;
        }
    }
}
