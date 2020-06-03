using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Client
{
    public class AllFields
    {
        private readonly string whiteColor = "#ffffff";
        public List<Field> ListOfFields = new List<Field>();

        public static int TopLineY = 113;
        public static int RightLineX = 893;
        public static int BottomLineY = 673;
        public static int LeftLineX = 333;

        public Point StartField = new Point(LeftLineX, TopLineY);
        public Point PrisonField = new Point(874, 138);
        public Point JailField = new Point(915, 92);
        public Point CasinoField = new Point(RightLineX, BottomLineY);
        public Point PolicemanField = new Point(LeftLineX, BottomLineY);

        public void ReadFromFile()
        {
            string path = @"D:\Oles\БГУИР\2 курс\4 сем\КСиС\Monopoly\Client\Client\Fields\FieldsInfo.txt";
            using (StreamReader streamReader = new StreamReader(path, Encoding.Default))
            {
                string line;
                while ((line = streamReader.ReadLine()) != null)
                {
                    string[] listOfWords = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    Field field = new Field();
                    field.Name = listOfWords[0];
                    field.CurrentRent = int.Parse(listOfWords[1]);
                    field.Star1 = int.Parse(listOfWords[2]);
                    field.Star2 = int.Parse(listOfWords[3]);
                    field.Star3 = int.Parse(listOfWords[4]);
                    field.Star4 = int.Parse(listOfWords[5]);
                    field.BigStar = int.Parse(listOfWords[6]);
                    field.Price = int.Parse(listOfWords[7]);
                    field.Deposit = int.Parse(listOfWords[8]);
                    field.Buyout = int.Parse(listOfWords[9]);
                    field.StarPrice = int.Parse(listOfWords[10]);
                    field.OwnerColor = whiteColor;
                    ListOfFields.Add(field);
                }
            }
        }

        public AllFields()
        {
            ReadFromFile();
        }

        public List<Point> ListOfCoordinates = new List<Point>()
        {
            new Point(LeftLineX, TopLineY), new Point(409, TopLineY), new Point(460, TopLineY), new Point(511, TopLineY), new Point(562, TopLineY),
            new Point(613, TopLineY), new Point(664, TopLineY), new Point(715, TopLineY), new Point(766, TopLineY), new Point(817, TopLineY),

            new Point(915, 92), new Point(RightLineX, 189), new Point(RightLineX, 240), new Point(RightLineX, 291), new Point(RightLineX, 342),
            new Point(RightLineX, 393), new Point(RightLineX, 444), new Point(RightLineX, 495), new Point(RightLineX, 546), new Point(RightLineX, 597),

            new Point(RightLineX, BottomLineY), new Point(817, BottomLineY), new Point(766, BottomLineY), new Point(715, BottomLineY), new Point(664, BottomLineY),
            new Point(613, BottomLineY), new Point(562, BottomLineY), new Point(511, BottomLineY), new Point(460, BottomLineY), new Point(409, BottomLineY),

            new Point(LeftLineX, BottomLineY), new Point(LeftLineX, 597), new Point(LeftLineX, 546), new Point(LeftLineX, 495), new Point(LeftLineX, 444),
            new Point(LeftLineX, 393), new Point(LeftLineX, 342), new Point(LeftLineX, 291), new Point(LeftLineX, 240), new Point(LeftLineX, 189)
        };
    }
}
