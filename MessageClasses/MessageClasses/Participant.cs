using System;
using System.Windows;

namespace MessageClasses
{
    [Serializable]
    public class Participant
    {
        public string name;
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

        public Participant(string name, int id, int CurrentPositionX, int CurrentPositionY, int CurrentFieldIndex, int FinalFieldIndex, int Money, string Color, int LeftCube, int RightCube, int movesLeft)
        {
            this.name = name;
            this.id = id;
            this.CurrentPositionX = CurrentPositionX;
            this.CurrentPositionY = CurrentPositionY;
            this.CurrentFieldIndex = CurrentFieldIndex;
            this.FinalFieldIndex = FinalFieldIndex;
            this.Money = Money;
            this.Color = Color;
            this.movesLeft = movesLeft;
        }
    }
}
