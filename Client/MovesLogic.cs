using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows.Media;
using MessageClasses;

namespace Client
{
    public class MovesLogic : PlayerData
    {
        public AllFields fields;

        public Point StartPlayer1 = new Point(306, 90);
        public Point StartPlayer2 = new Point(360, 138);
        public Point StartPlayer3 = new Point(360, 90);
        public Point StartPlayer4 = new Point(306, 138);

        public int finalFieldX;
        public int finalFieldY;
        
        private string whiteColor = "#ffffff";

        public int currentIndex;
        public PlayerData client;
        public Participant currentPlayer;
        public List<Field> ListOfFields;
        public List<Participant> ListOfPlayers;

        const int JAIL = 10;
        const int PRISON = 30;

        public MovesLogic(MainWindow mainWindow) : base(mainWindow) {}

        public void Add(List<Field> ListOfFields, List<Participant> ListOfPlayers, PlayerData client)
        {
            this.client = client;
            this.ListOfFields = ListOfFields;
            this.ListOfPlayers = ListOfPlayers;
        }

        public void Initialize(AllFields fields, Participant currentPlayer, int currentIndex)
        {
            this.currentPlayer = currentPlayer;
            this.fields = fields;
            this.currentIndex = currentIndex;
        }

        public bool ItIsFirstPlayerMove()
        {
            if (((currentPlayer.CurrentPositionX == StartPlayer1.X) && (currentPlayer.CurrentPositionY == StartPlayer1.Y))
                || (currentPlayer.CurrentPositionX == StartPlayer2.X) && (currentPlayer.CurrentPositionY == StartPlayer2.Y) 
                || ((currentPlayer.CurrentPositionX == StartPlayer3.X) && (currentPlayer.CurrentPositionY == StartPlayer3.Y))
                || ((currentPlayer.CurrentPositionX == StartPlayer4.X) && (currentPlayer.CurrentPositionY == StartPlayer4.Y))) return true;
            else return false;
        }

        public bool PlayerOnTopLine()
        {
            if ((currentPlayer.CurrentFieldIndex >= 0) && (currentPlayer.CurrentFieldIndex <= 9) 
                && (currentPlayer.FinalFieldIndex <= 20)) return true;
            else return false;
        }

        public bool PlayerOnRightLine()
        {
            if ((currentPlayer.CurrentFieldIndex >= JAIL) && (currentPlayer.CurrentFieldIndex <= 19) 
                && (currentPlayer.FinalFieldIndex <= PRISON)) return true;
            else return false;
        }

        public bool PlayerOnBottomLine()
        {
            if ((currentPlayer.CurrentFieldIndex >= 20) && (currentPlayer.CurrentFieldIndex <= 29) 
                && (currentPlayer.FinalFieldIndex <= 39)) return true;
            else return false;
        }

        public bool PlayerOnLeftLine()
        {
            if ((currentPlayer.CurrentFieldIndex >= PRISON) && (currentPlayer.CurrentFieldIndex <= 39)
                 && (currentPlayer.FinalFieldIndex >= 0)) return true;
            else return false;
        }

        public bool ChanceField()
        {
            bool flag = false;
            List<int> chanceFieldIndex = new List<int>() { 2, 7, 17, 22, 33, 38};
            foreach (var field in chanceFieldIndex)
            {
                if (currentPlayer.FinalFieldIndex == field)
                    return true;
            }
            return flag;
        }

        public void MovePlayer(Point finalPoint, Ellipse player)
        {
            finalFieldX = (int)finalPoint.X;
            finalFieldY = (int)finalPoint.Y;

            var x = Canvas.GetLeft(player);
            var y = Canvas.GetTop(player);

            DispatcherTimer timer;
            timer = new DispatcherTimer();
            timer.Tick += new EventHandler(timer_Tick);
            timer.Interval = new TimeSpan(10000);
            timer.Start();

            void MoveRight()
            {
                if (finalFieldX > x) x += 5;
                else x = finalFieldX;
                Canvas.SetLeft(player, x);
            }

            void MoveDown()
            {
                if (finalFieldY > y) y += 5;
                else y = finalFieldY;
                Canvas.SetTop(player, y);
            }

            void MoveLeft()
            {
                if (finalFieldX < x) x -= 5;
                else x = finalFieldX;
                Canvas.SetLeft(player, x);
            }

            void MoveUp()
            {
                if (finalFieldY < y) y -= 5;
                else y = finalFieldY;
                Canvas.SetTop(player, y);
            }

            void TopLineMove()
            {
                if ((currentPlayer.CurrentFieldIndex == 9) && (currentPlayer.FinalFieldIndex == 21))
                {
                    if ((int)x != AllFields.RightLineX) MoveRight();
                    else if ((int)y != AllFields.BottomLineY) MoveDown();
                    else MoveLeft();
                }
                else if (ItIsFirstPlayerMove())
                {
                    if ((int)x != finalFieldX) MoveRight();
                    else if ((int)y != finalFieldY) MoveDown();
                    else if (fields.ListOfCoordinates[currentPlayer.FinalFieldIndex] == fields.JailField)
                    {
                        if (y > finalFieldY) MoveUp();
                        else MoveDown();
                    }
                }
                else if ((int)x != finalFieldX) MoveRight();
                else if (fields.ListOfCoordinates[currentPlayer.FinalFieldIndex] == fields.JailField) MoveUp();
                else if ((int)y != finalFieldY) MoveDown();
                /*
                MoveRight();
                if (ItIsFirstPlayerMove() && (x == finalFieldX)) MoveDown();
                else if (fields.ListOfCoordinates[currentPlayer.FinalFieldIndex] == fields.JailField) MoveUp();
                else if ((currentPlayer.CurrentFieldIndex == 9) && (currentPlayer.FinalFieldIndex == 21))
                {
                    MoveDown();
                    if (y == finalFieldY) MoveLeft();
                }
                else if (x == finalFieldX) MoveDown();
                */
            }

            void RightLineMove()
            {
                MoveDown();
                if ((currentPlayer.CurrentFieldIndex == 19) && (currentPlayer.FinalFieldIndex == 31))
                {
                    MoveLeft();
                    if (x == finalFieldX) MoveUp();
                }
                else if (y == finalFieldY) MoveLeft();
            }

            void BottomLineMove()
            {
                MoveLeft();
                if ((CurrentFieldIndex == 29) && (FinalFieldIndex == 1))
                {
                    MoveUp();
                    if (y == finalFieldY) MoveRight();
                }
                else if (x == finalFieldX) MoveUp();
            }

            void LeftLineMove()
            {
                if ((CurrentFieldIndex == 39) && (FinalFieldIndex == 11))
                {
                    if (y != AllFields.TopLineY) MoveUp();
                    else if (y == AllFields.TopLineY) MoveRight();
                    else MoveDown();
                }
                else if (CurrentFieldIndex == PRISON)
                {
                    MoveUp();
                    if (y == finalFieldY) MoveRight();
                }
                else if (y != finalFieldY) MoveUp();
                else MoveRight();
            }

            void HideCubes()
            {
                mainWindow.Cube1.Visibility = Visibility.Hidden;
                mainWindow.Cube2.Visibility = Visibility.Hidden;
            }

            void ShowTrowCubesPanel()
            {
                mainWindow.ThrowCubesPanel.Visibility = Visibility.Visible;
            }

            void ShowChancePanel(int number)
            {
                fields.ListOfFields[currentPlayer.FinalFieldIndex].CurrentRent = number;

                int playerMoney = currentPlayer.Money;

                if ((playerMoney -= number) >= 0) mainWindow.PayFieldButton.IsEnabled = true;
                else mainWindow.PayFieldButton.IsEnabled = false;
                mainWindow.PayFieldButton.Content = "Заплатить " + number.ToString() + "k";
                ShowPayPanel();
            }

            void ShowCasinoPanel()
            {
                mainWindow.CasinoPanel.Visibility = Visibility.Visible;
            }

            void ShowBuyPanel()
            {
                mainWindow.BuyFieldPanel.Visibility = Visibility.Visible;
                mainWindow.BuyFieldButton.Content = "Купить за " + ListOfFields[currentPlayer.FinalFieldIndex].Price.ToString() + "k";
            }

            void ShowPayPanel()
            {
                mainWindow.PayFieldPanel.Visibility = Visibility.Visible;
            }

            bool JailField()
            {
                if ((currentPlayer.FinalFieldIndex == JAIL) && (currentPlayer.CurrentFieldIndex != PRISON)) return true;
                else return false;
            }

            bool PrisonField()
            {
                if ((currentPlayer.FinalFieldIndex == JAIL) && (currentPlayer.CurrentFieldIndex == PRISON)) return true;
                else return false;
            }

            bool CanBuyField()
            {
                if (ListOfFields[currentPlayer.FinalFieldIndex].Price != 0) return true;
                else return false;
            }

            bool FieldHasOwner()
            {
                if (ListOfFields[currentPlayer.FinalFieldIndex].OwnerColor != whiteColor) return true;
                else return false;
            }

            bool PayTaxes()
            {
                bool flag = false;
                if ((currentPlayer.FinalFieldIndex == 4) || (currentPlayer.FinalFieldIndex == 36)) 
                {
                    return true;
                }
                return flag;
            }

            bool PlayerInCasino()
            {
                if (currentPlayer.FinalFieldIndex == 20) return true;
                else return false;
            }

            void timer_Tick(object sender, EventArgs e)
            {
                string message = "";
                if ((finalFieldX == x) && (finalFieldY == y))
                {
                    timer.Stop();
                    HideCubes();
                    if (currentPlayer.FinalFieldIndex == PRISON)
                    {
                        currentPlayer.CurrentFieldIndex = currentPlayer.FinalFieldIndex;
                        currentPlayer.FinalFieldIndex = JAIL; 
                        if (currentIndex == 0) MovePlayer(fields.PrisonField, mainWindow.player1Chip);
                        else if (currentIndex == 1) MovePlayer(fields.PrisonField, mainWindow.player2Chip);
                        else if (currentIndex == 2) MovePlayer(fields.PrisonField, mainWindow.player3Chip);
                        else MovePlayer(fields.PrisonField, mainWindow.player4Chip);
                    }
                    if (client.Index == currentPlayer.id)
                    {
                        if (CanBuyField())
                        {
                            if (FieldHasOwner())
                            {
                                if (ListOfFields[currentPlayer.FinalFieldIndex].OwnerColor == currentPlayer.Color)
                                {
                                    message = currentPlayer.name + " попадает на своё поле";
                                    client.SendDescriptionMessage(message);

                                    if (mainWindow.NoMovesLeft()) mainWindow.NextPlayer();
                                    else if (client.Index == currentPlayer.id) mainWindow.ShowThrowCubesPanel();
                                }
                                else
                                {
                                    int playerMoney = currentPlayer.Money;

                                    if ((currentPlayer.FinalFieldIndex != 12) && (currentPlayer.FinalFieldIndex != 28))
                                    {
                                        if ((playerMoney -= ListOfFields[currentPlayer.FinalFieldIndex].CurrentRent) >= 0) mainWindow.PayFieldButton.IsEnabled = true;
                                        else mainWindow.PayFieldButton.IsEnabled = false;
                                        mainWindow.PayFieldButton.Content = "Заплатить " + fields.ListOfFields[currentPlayer.FinalFieldIndex].CurrentRent + "k";
                                    }
                                    else
                                    {
                                        int gamesRent = (currentPlayer.LeftCube + currentPlayer.RightCube) * fields.ListOfFields[currentPlayer.FinalFieldIndex].CurrentRent;
                                        mainWindow.PayFieldButton.Content = "Заплатить " + gamesRent + "k";
                                        fields.ListOfFields[currentPlayer.FinalFieldIndex].CurrentRent = gamesRent;
                                        if ((playerMoney -= gamesRent) >= 0) mainWindow.PayFieldButton.IsEnabled = true;
                                        else mainWindow.PayFieldButton.IsEnabled = false;
                                    }

                                    foreach (var playerInfo in client.ListOfParticipants)
                                    {
                                        if (playerInfo.Color == fields.ListOfFields[currentPlayer.FinalFieldIndex].OwnerColor)
                                        {
                                            message = currentPlayer.name + " попадает на " + fields.ListOfFields[currentPlayer.FinalFieldIndex].Name + " и должен заплатить игроку " + playerInfo.name + " аренду в размере " + fields.ListOfFields[currentPlayer.FinalFieldIndex].CurrentRent + "k";
                                            client.SendDescriptionMessage(message);

                                            ShowPayPanel();
                                        }
                                    }
                                }
                            }
                            else
                            {
                                message = currentPlayer.name + " попадает на " + fields.ListOfFields[currentPlayer.FinalFieldIndex].Name + " и задумывается о покупке";
                                client.SendDescriptionMessage(message);

                                int playerMoney = currentPlayer.Money;
                                if ((playerMoney -= ListOfFields[currentPlayer.FinalFieldIndex].Price) >= 0) mainWindow.BuyFieldButton.IsEnabled = true;
                                else mainWindow.BuyFieldButton.IsEnabled = false;
                                ShowBuyPanel();
                            }
                        }
                        else if (ChanceField())
                        {
                            Random random = new Random();
                            int value = random.Next(0, 2);
                            int valueNumber = random.Next(500, 2501);

                            message = currentPlayer.name + " попадает на поле \"Шанс\". ";

                            if (value == 0)
                            {
                                message += currentPlayer.name + " должен оплатить расходы на образование в размере " + valueNumber.ToString() + "k";
                                client.SendDescriptionMessage(message);

                                int playerMoney = currentPlayer.Money;
                                if ((playerMoney -= ListOfFields[currentPlayer.FinalFieldIndex].Price) >= 0) mainWindow.PayFieldButton.IsEnabled = true;
                                else mainWindow.PayFieldButton.IsEnabled = false;
                                ShowChancePanel(valueNumber);
                            }
                            else
                            {
                                currentPlayer.Money += valueNumber;

                                message += "Банк выплачивает игроку " + currentPlayer.name + " дивиденды на сумму в " + valueNumber.ToString() + "k";
                                client.SendDescriptionMessage(message);

                                mainWindow.SendMoneyMessage();
                            }
                        }
                        else if (JailField())
                        {
                            message = currentPlayer.name + " посещает полицейский участок с экскурсией";
                            client.SendDescriptionMessage(message);

                            if (mainWindow.NoMovesLeft()) mainWindow.NextPlayer();
                            else if (client.Index == currentPlayer.id) ShowTrowCubesPanel();
                        }
                        else if (PayTaxes())
                        {
                            fields.ListOfFields[currentPlayer.FinalFieldIndex].CurrentRent = 2000;
                            message = currentPlayer.name + " попадает на поле \"Налог\" и должен заплатить Банку " + fields.ListOfFields[currentPlayer.FinalFieldIndex].CurrentRent + "k";
                            client.SendDescriptionMessage(message);

                            mainWindow.PayFieldButton.Content = "Заплатить " + fields.ListOfFields[currentPlayer.FinalFieldIndex].CurrentRent + "k";
                            ShowPayPanel();
                        }
                        else if (PlayerInCasino())
                        {
                            message = currentPlayer.name + " попадает на поле \"Казино\"";
                            client.SendDescriptionMessage(message);

                            ShowCasinoPanel();
                        }
                        else if (PrisonField())
                        {
                            if (currentPlayer.CurrentFieldIndex != 10)
                            {
                                message = currentPlayer.name + " арестован полицией и отправляется в тюрьму";
                                client.SendDescriptionMessage(message);

                                currentPlayer.movesLeft = 0;

                                mainWindow.NextPlayer();
                                currentPlayer.FinalFieldIndex = 10;
                                currentPlayer.CurrentFieldIndex = 10;
                            }
                        }
                        else if ((currentPlayer.CurrentFieldIndex <= 39) && (currentPlayer.CurrentFieldIndex != PRISON) 
                            && (currentPlayer.CurrentFieldIndex >= 19) && (currentPlayer.FinalFieldIndex >= 0) 
                            && ((currentPlayer.FinalFieldIndex <= 11)) && (!ItIsFirstPlayerMove()))
                        {
                            int LapBonus = 2000;
                            int startBonus = 1000;

                            currentPlayer.Money += LapBonus;
                            message = currentPlayer.name + " проходит очередной круг и получает " + LapBonus + "k";

                            if (currentPlayer.FinalFieldIndex == 0)
                            {
                                currentPlayer.Money += startBonus;
                                message += ". " + currentPlayer.name + " останавливается на поле \"Старт\" и получает бонус в размере" + startBonus.ToString() + "k";
                            }
                            client.SendDescriptionMessage(message);
                            mainWindow.SendMoneyMessage();
                        }
                    }
                }
                else if (PlayerOnTopLine()) TopLineMove();
                else if (PlayerOnRightLine()) RightLineMove();
                else if (PlayerOnBottomLine()) BottomLineMove();
                else if (PlayerOnLeftLine()) LeftLineMove(); 
            }
        }
    }
}
