using System;
using System.Net;

namespace MessageClasses
{
    [Serializable]
    public class TcpRequestMessage : Message
    {
        public string clientName;
        public int id;
        public int CurrentPositionX;
        public int CurrentPositionY;
        public int CurrentFieldIndex;
        public int FinalFieldIndex;
        public int Money;
        public string Color;
        public int LeftCube;
        public int RightCube;
        public int movesLeft;

        public TcpRequestMessage(IPAddress ip, string clientName, int CurrentPositionX, int CurrentPositionY, int CurrentFieldIndex, int FinalFieldIndex, int Money, string Color, int LeftCube, int RightCube, int movesLeft) : base(ip)
        {
            this.clientName = clientName;
            this.CurrentPositionX = CurrentPositionX;
            this.CurrentPositionY = CurrentPositionY;
            this.CurrentFieldIndex = CurrentFieldIndex;
            this.FinalFieldIndex = FinalFieldIndex;
            this.Money = Money;
            this.Color = Color;
            this.LeftCube = LeftCube;
            this.RightCube = RightCube;
            this.movesLeft = movesLeft;
        }
    }
}
