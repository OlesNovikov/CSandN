using MessageClasses;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Client
{
    public partial class MainWindow : Window
    {
        MovesLogic logic;
        PlayerData player;
        Participant currentPlayer;
        Random value = new Random();
        int currentIndex = 0;
        //int RandomNumber;
        //Cube cube = new Cube();
        public AllFields fields;
        public List<Participant> ListOfPlayers = new List<Participant>();
        public StackPanel Profile;

        string TextBoxHint = "Enter message";
        string whiteColor = "#ffffff";

        public MainWindow()
        {
            InitializeComponent();
        }

        public void InitializeFields()
        {
            fields = new AllFields();
            for (int index = 0; index < fields.ListOfFields.Count; index++)
            {
                if (fields.ListOfFields[index].Price != 0)
                {
                    if ((index < 10) || ((index > 20) && (index < 30))) ((TextBlock)FindName(fields.ListOfFields[index].Name)).Text = fields.ListOfFields[index].Price.ToString() + "k";
                    else ((TabItem)FindName(fields.ListOfFields[index].Name)).Header = "  " + fields.ListOfFields[index].Price.ToString() + "k";
                }
            }
        }

        private void HideChips()
        {
            player1Chip.Visibility = Visibility.Hidden;
            player2Chip.Visibility = Visibility.Hidden;
            player3Chip.Visibility = Visibility.Hidden;
            player4Chip.Visibility = Visibility.Hidden;
        }

        private void MainWindowLoaded(object sender, RoutedEventArgs e)
        {
            player = new PlayerData(this);
            player.SendUdpRequest();

            player.ListOfParticipantsReceivedEvent += ShowAllPlayerProfiles;
            player.StartGameMessageReceivedEvent += ShowThrowCubesPanel;
            player.PublicMessageReceivedEvent += ReceivePublicMessage;
            player.DescriptionMessageReceivedEvent += ReceiveMoveDescriptionMessage;
            player.CubeMessageReceivedEvent += ShowCubesAndMove;
            player.MoneyMessageReceivedEvent += UpdateAllPlayersMoney;
            player.MoveMessageReceivedEvent += SetupNextPlayer;
            player.CasinoMessageReceivedEvent += HandleCasinoEvent;
            player.DisconnectMessageReceivedEvent += RedrawFieldsAndPlayers;

            InitializeFields();
            HideChips();
            MessageTextBox.Text = TextBoxHint;
        }

        public void RedrawFieldsAndPlayers(DisconnectMessage message)
        {
            Action redraw = delegate
            {
                player.ListOfParticipants = message.ListOfParticipants;
                if (message.id == 0) player1Chip.Visibility = Visibility.Hidden;
                else if (message.id == 1) player2Chip.Visibility = Visibility.Hidden;
                else if (message.id == 2) player3Chip.Visibility = Visibility.Hidden;
                else player4Chip.Visibility = Visibility.Hidden;

                for (int i = 0; i < fields.ListOfFields.Count; i++)
                {
                    if (fields.ListOfFields[i].Price != 0)
                    {
                        var field = fields.ListOfFields[i];

                        if (field.OwnerColor == message.Color)
                        {
                            fields.ListOfFields[i].OwnerColor = whiteColor;
                            if ((i < 10) || ((i > 20) && (i < 30)))
                            {
                                ((TextBlock)FindName(fields.ListOfFields[i].Name)).Text = fields.ListOfFields[i].Price.ToString() + "k";
                                ((TextBlock)FindName(fields.ListOfFields[i].Name)).Background = BrushFromHex(whiteColor);
                            }
                            else
                            {
                                ((TabItem)FindName(fields.ListOfFields[i].Name)).Header = "  " + fields.ListOfFields[i].Price.ToString() + "k";
                                ((TabItem)FindName(fields.ListOfFields[i].Name)).Background = BrushFromHex(whiteColor);
                            }
                        }
                    }
                }
                if (message.id == message.currentPlayerId)
                {
                    if (message.id == player.ListOfParticipants.Count)
                    {
                        currentIndex = 0;
                        currentPlayer = player.ListOfParticipants[currentIndex];
                    }
                }
                else
                {
                    for (int i = 0; i < player.ListOfParticipants.Count; i++)
                    {
                        if (message.currentPlayerId == player.ListOfParticipants[i].id)
                        {
                            currentPlayer = player.ListOfParticipants[i];
                            currentIndex = i;
                        }
                    }
                }
            };

            player1Chip.Dispatcher.Invoke(redraw);
        }

        private void MainWindowClosed(object sender, EventArgs e)
        {
            if (player.tcpListenSocket != null)
            {
                player.ListOfParticipants.RemoveAt(player.Index);

                DisconnectMessage disconnectMessage = new DisconnectMessage(player.clientIp, player.ListOfParticipants, player.Index, player.Color, currentPlayer.id);
                player.SendDisconnectMessage(disconnectMessage);
                player.DisconnectClient();
            }
        }

        public void ShowAllPlayerProfiles()
        {
            Action showProfiles = delegate
            {
                AllPlayersProfiles.Children.Clear();
                foreach (var participant in player.ListOfParticipants)
                {
                    if (player.Index == participant.id) player.Color = participant.id.ToString();
                    if (participant.id == 0) Profile = new StackPanel { Name = "Player" + participant.id.ToString(), Width = 160, Height = 120, Margin = new Thickness(0, 0, 0, 0), Background = BrushFromHex("#161a1b") };
                    else Profile = new StackPanel { Name = "Player" + participant.id.ToString(), Width = 160, Height = 120, Margin = new Thickness(0, 30, 0, 0), Background  = BrushFromHex("#161a1b") };

                    BitmapImage profileImage = new BitmapImage();
                    profileImage.BeginInit();
                    profileImage.UriSource = new Uri(@"Images/Field parts/Profile1.bmp", UriKind.RelativeOrAbsolute);
                    profileImage.EndInit();
                    Profile.Children.Add(new Image() { Name = "PlayerImage" + participant.id.ToString(), Height = 50, Width = 50, Source = profileImage, Margin = new Thickness(0, 15, 0, 0) });
                    Profile.Children.Add(new TextBlock() { Name = "Nickname" + participant.id.ToString(), Height = 20, Width = 70, Text = participant.name, TextAlignment = TextAlignment.Center, Foreground = BrushFromHex("#FFFFFFFF"), FontSize = 12, Margin = new Thickness(0, 5, 0, 0) });
                    Profile.Children.Add(new TextBlock() { Name = "Money" + participant.id.ToString(), Height = 20, Width = 70, Text = participant.Money.ToString() + "k", TextAlignment = TextAlignment.Center, Foreground = BrushFromHex("#FFFFFFFF"), FontSize = 18, Margin = new Thickness(0, 0, 0, 0) });
                    AllPlayersProfiles.Children.Add(Profile);

                    if (participant.id == 0) player1Chip.Visibility = Visibility.Visible;
                    else if (participant.id == 1) player2Chip.Visibility = Visibility.Visible;
                    else if (participant.id == 2) player3Chip.Visibility = Visibility.Visible;
                    else player4Chip.Visibility = Visibility.Visible;
                }

                RegistartionPanelBorder.Visibility = Visibility.Collapsed;
                CanvasMap.Visibility = Visibility.Visible;

                if (player.ListOfParticipants.Count == 4)
                {
                    var panel = (StackPanel)AllPlayersProfiles.Children[0];
                    panel.Background = BrushFromHex(player.ListOfParticipants[0].Color);
                    if (player.Index == 0)
                    {
                        ShowThrowCubesPanel();
                    }
                }
            };
            AllPlayersProfiles.Dispatcher.Invoke(showProfiles);
        }

        public void ShowThrowCubesPanel()
        {
            Action showCubesPanel = delegate
            {
                ThrowCubesPanel.Visibility = Visibility.Visible;
            };

            ThrowCubesPanel.Dispatcher.Invoke(showCubesPanel);
        }

        public void SetupNextPlayer(MoveMessage message)
        {
            Action next = delegate
            {
                for (int i = 0; i < player.ListOfParticipants.Count; i++)
                {
                    if (message.id == player.ListOfParticipants[i].id)
                    {
                        currentPlayer = player.ListOfParticipants[i];
                        currentIndex = i;
                        currentPlayer.movesLeft = 1;
                    }
                }

                for (int i = 0; i < AllPlayersProfiles.Children.Count; i++)
                {
                    var stackPanel = (StackPanel)AllPlayersProfiles.Children[i];
                    stackPanel.Background = BrushFromHex("#161a1b");
                }

                StackPanel panel = (StackPanel)AllPlayersProfiles.Children[currentPlayer.id];
                panel.Background = BrushFromHex(currentPlayer.Color);
                //if (currentPlayer.movesLeft == 0) currentPlayer.movesLeft++;
                //MessageBox.Show(currentPlayer.name);
                if (message.id == player.Index)
                {
                    ShowThrowCubesPanel();
                }
                //MessageBox.Show("Client id: " + player.Index.ToString() + " message id: " + message.id.ToString()); 
            };

            ThrowCubesPanel.Dispatcher.Invoke(next);
            /*
            if (currentPlayer.movesLeft == 0) currentPlayer.movesLeft++;
            currentIndex++;
            currentIndex %= 4;
            currentPlayer = player.ListOfParticipants[currentIndex];*/
        }

        public void ReceivePublicMessage(PublicMessage publicMessage)
        {
            string message = publicMessage.senderName + ": " + publicMessage.data;
            ShowPublicMessage(message);
        }

        public void ReceiveMoveDescriptionMessage(DescriptionMessage descriptionMessage)
        {
            string message = descriptionMessage.data;
            ShowPublicMessage(message);
        }

        public void ShowPublicMessage(string message)
        {
            Action showMessage = delegate
            {
                txtChat.AppendText(message + Environment.NewLine);
                txtChat.ScrollToEnd();
            };
            txtChat.Dispatcher.Invoke(showMessage);
        }

        public void ShowCubesAndMove()
        {
            int cubeSum = 0;

            Action showCubes = delegate
            {
                string message = "";
                ThrowCubesPanel.Visibility = Visibility.Hidden;

                Cube1.Source = ImageSource(player.LeftCube);
                Cube2.Source = ImageSource(player.RightCube);

                Cube1.Visibility = Visibility.Visible;
                Cube2.Visibility = Visibility.Visible;

                currentPlayer = player.ListOfParticipants[currentIndex];

                currentPlayer.LeftCube = player.LeftCube;
                currentPlayer.RightCube = player.RightCube;
                cubeSum = currentPlayer.LeftCube + currentPlayer.RightCube;

                if (player.Index == currentPlayer.id)
                {
                    message = currentPlayer.name + " выбрасывает " + currentPlayer.LeftCube.ToString() + ":" + currentPlayer.RightCube.ToString();
                    player.SendDescriptionMessage(message);
                }
                
                currentPlayer.movesLeft--;

                //MessageBox.Show(currentPlayer.name + " id: " + currentPlayer.id.ToString() + player.Index.ToString());

                if (currentPlayer.RightCube == currentPlayer.LeftCube)
                {
                    if (player.Index == currentPlayer.id)
                    {
                        message = currentPlayer.name + " выбрасывает " + currentPlayer.LeftCube.ToString() + ":" + currentPlayer.RightCube.ToString() + " и получает ещё один ход, так как выпал дубль";
                        player.SendDescriptionMessage(message);
                    }
                    currentPlayer.movesLeft++;
                    //player.ListOfParticipants[currentPlayer.id].movesLeft++;
                }

                currentPlayer.CurrentFieldIndex = currentPlayer.FinalFieldIndex;
                currentPlayer.FinalFieldIndex = (currentPlayer.FinalFieldIndex + cubeSum) % 40;

                logic.Add(fields.ListOfFields, player.ListOfParticipants, player);
                logic.Initialize(fields, currentPlayer, currentIndex);

                Point MovePlayerTo = fields.ListOfCoordinates[currentPlayer.FinalFieldIndex];

                if (currentIndex == 0) currentIndex = logic.MovePlayer(MovePlayerTo, player1Chip);
                else if (currentIndex == 1) currentIndex = logic.MovePlayer(MovePlayerTo, player2Chip);
                else if (currentIndex == 2) currentIndex = logic.MovePlayer(MovePlayerTo, player3Chip);
                else currentIndex = logic.MovePlayer(MovePlayerTo, player4Chip);
            };
            Dispatcher.Invoke(showCubes);
        }

        public void UpdateAllPlayersMoney()
        {
            Field fieldToBuy = fields.ListOfFields[currentPlayer.FinalFieldIndex];
            string componentName = fieldToBuy.Name;

            Action updateMoney = delegate
            {
                foreach (var participant in player.ListOfParticipants)
                {
                    StackPanel panel = (StackPanel)AllPlayersProfiles.Children[participant.id];
                    ((TextBlock)panel.Children[2]).Text = participant.Money.ToString() + "k";
                }

                if (!PayToBank() && !ChanceField() && !PayRent() && !Casino()) ProcessPurchase(componentName, fieldToBuy);

                if (player.Index == currentPlayer.id)
                {
                    if (NoMovesLeft()) NextPlayer();
                    else ShowThrowCubesPanel();
                }
            };
            AllPlayersProfiles.Dispatcher.Invoke(updateMoney);
        }

        public bool Casino()
        {
            if (currentPlayer.FinalFieldIndex == 20) return true;
            else return false;
        }

        public void ProcessPurchase(string componentName, Field fieldToBuy)
        {
            string fieldRent = fieldToBuy.CurrentRent.ToString();
            fieldToBuy.OwnerColor = currentPlayer.Color;
            if (PlayerOnTopOrBottomLine())
            {
                ((TextBlock)FindName(componentName)).Background = BrushFromHex(currentPlayer.Color);

                if (currentPlayer.FinalFieldIndex != 28) ((TextBlock)FindName(componentName)).Text = fieldRent + "k";
                else ((TextBlock)FindName(componentName)).Text = fieldRent + "x";
            }
            else
            {
                ((TabItem)FindName(componentName)).Background = BrushFromHex(currentPlayer.Color);

                if (currentPlayer.FinalFieldIndex != 12) ((TabItem)FindName(componentName)).Header = "  " + fieldRent + "k";
                else ((TabItem)FindName(componentName)).Header = "  " + fieldRent + "x";
            }
        }

        public void HideThrowCubesPanel()
        {
            ThrowCubesPanel.Visibility = Visibility.Hidden;
        }

        public SolidColorBrush BrushFromHex(string hexColorString) 
        {
            return (SolidColorBrush)(new BrushConverter().ConvertFrom(hexColorString));
        }

        public void ShowMessageTextBoxHint()
        {
            MessageTextBox.Text = TextBoxHint;
            MessageTextBox.Foreground = BrushFromHex("#656d78");
        }

        private void MessageTextBox_Enter(object sender, RoutedEventArgs e)
        {
            if ((MessageTextBox.Text.Length == 0) || (MessageTextBox.Text == TextBoxHint))
            {
                MessageTextBox.Text = null;
                MessageTextBox.Foreground = BrushFromHex(whiteColor);
            }
        }

        private void MessageTextBox_Leave(object sender, RoutedEventArgs e)
        {
            if (MessageTextBox.Text.Length == 0)
            {
                ShowMessageTextBoxHint();
            }
        }

        private void SendMessage_Click(object sender, RoutedEventArgs e)
        {
            if (MessageTextBox.Text != TextBoxHint)
            {
                string message = MessageTextBox.Text;
                player.SendPublicMessage(message);
                //txtChat.AppendText(MessageTextBox.Text + Environment.NewLine);
                ShowMessageTextBoxHint();
            }
        }

        public void InitializeNewPlayer()
        {
            player.Name = RegistrationTextBox.Text;
            player.Money = 15000;
            //player.Color = player.ListOfParticipants.Count.ToString();
            player.CurrentFieldIndex = 0;
            player.FinalFieldIndex = 0;
            player.movesLeft = 1;
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            logic = new MovesLogic(this);
            InitializeNewPlayer();
            //int profiles = player.ListOfParticipants.Count;
            //int profiles = ListOfPlayers.Count;
            //SetVisibilityProperties();
            //ListOfPlayers.Add(player.ListOfParticipants[currentIndex]);
            player.SendTcpRequest();
        }
        
        public BitmapImage ImageSource(int randNumber)
        {
            //RandomNumber = cube.SetValueToCube(value);
            string CubeSource = randNumber.ToString() + ".bmp";

            BitmapImage myCubeImage = new BitmapImage();
            myCubeImage.BeginInit();
            myCubeImage.UriSource = new Uri(@"Images/Coub/" + CubeSource, UriKind.RelativeOrAbsolute);
            myCubeImage.EndInit();
            return myCubeImage;
        }

        public bool PlayerOnTopOrBottomLine()
        {
            if ((currentPlayer.FinalFieldIndex < 10)
                || ((currentPlayer.FinalFieldIndex > 20) && (currentPlayer.FinalFieldIndex < 30))) return true;
            else return false;
        }

        private void BuyFieldButton_Click(object sender, RoutedEventArgs e)
        {
            Field fieldToBuy = fields.ListOfFields[currentPlayer.FinalFieldIndex];
            string message = "";
            string componentName = fieldToBuy.Name;

            message = player.ListOfParticipants[currentPlayer.id].name + " покупает " + componentName + " за " + fieldToBuy.Price.ToString() + "k";
            player.SendDescriptionMessage(message);

            currentPlayer.Money -= fieldToBuy.Price;

            HideBuyFieldPanel();
            SendMoneyMessage();

            //if (NoMovesLeft()) NextPlayer();
            
            //ShowThrowCubesPanel();
        }

        public void SendMoneyMessage()
        {
            MoneyMessage moneyMessage = new MoneyMessage(player.clientIp, player.ListOfParticipants, DateTime.Now);
            player.SendMoneyMessage(moneyMessage);
        }

        public void PayToAnotherPlayer(int moneyToPay, Field currentField)
        {
            /*
            StackPanel panel = (StackPanel)AllPlayersProfiles.Children[currentIndex];
            ((TextBlock)panel.Children[2]).Text = currentPlayer.Money.ToString() + "k";*/

                foreach (var fieldOwner in player.ListOfParticipants)
                {
                    if (fieldOwner.Color == currentField.OwnerColor)
                    {
                        fieldOwner.Money += moneyToPay;
                        /*
                        panel = (StackPanel)AllPlayersProfiles.Children[fieldOwner.id];
                        ((TextBlock)panel.Children[2]).Text = fieldOwner.Money.ToString() + "k";*/
                    }
                }
            
        }

        public bool ChanceField()
        {
            bool flag = false;
            List<int> chanceFieldIndex = new List<int>() { 2, 7, 17, 22, 33, 38 };
            foreach (var field in chanceFieldIndex)
            {
                if (currentPlayer.FinalFieldIndex == field)
                    return true;
            }
            return flag;
        }

        public bool PayRent()
        {
            Field currentField = fields.ListOfFields[currentPlayer.FinalFieldIndex];
            if ((currentField.OwnerColor != currentPlayer.Color) && (currentField.OwnerColor != whiteColor)) return true;
            else return false;
        }

        private void PayFieldButton_Click(object sender, RoutedEventArgs e)
        {
            string message = "";
            int moneyToPay = 0;
            Field currentField = fields.ListOfFields[currentPlayer.FinalFieldIndex];

            moneyToPay = currentField.CurrentRent;

            currentPlayer.Money -= moneyToPay;

            if (!PayToBank() && !ChanceField() && PayRent()) PayToAnotherPlayer(moneyToPay, currentField);

            message = currentPlayer.name + " оплачивает расходы";
            player.SendDescriptionMessage(message);
            //txtChat.AppendText(message + Environment.NewLine);

            HidePayFieldPanel();
            SendMoneyMessage();
            //if (NoMovesLeft()) NextPlayer();
            //if (NoMovesLeft()) NextPlayer();
            //else ShowThrowCubesPanel();
        }

        public void NextPlayer()
        {
            if (currentPlayer.movesLeft == 0) player.ListOfParticipants[currentIndex].movesLeft++;
            //MessageBox.Show("Сейчас ходит: " + currentPlayer.name + " id: " + currentPlayer.id);
            currentIndex++;
            currentIndex %= player.ListOfParticipants.Count;
            currentPlayer = player.ListOfParticipants[currentIndex];
            //MessageBox.Show("Будет ходить: " + currentPlayer.name + " id: " + currentPlayer.id);
            player.SendNextPlayerMoveMessage(currentPlayer);
        }

        public bool PayToBank()
        {
            if ((currentPlayer.FinalFieldIndex == 4) || (currentPlayer.FinalFieldIndex == 36)) return true;
            else return false;
        }

        private void ThrowCubesButton_Click(object sender, RoutedEventArgs e)

        {
            //string message = "";
            /*
            currentPlayer = ListOfPlayers[currentIndex];
            if (currentPlayer.movesLeft == 0) NextPlayer();
            */
            //MessageBox.Show(currentIndex.ToString());
            currentPlayer = player.ListOfParticipants[currentIndex];
            //int CubeSum;
            //Point MovePlayerTo = new Point();

            if (currentIndex == 0)
            {
                currentPlayer.CurrentPositionX = (int)Canvas.GetLeft(player1Chip);
                currentPlayer.CurrentPositionY = (int)Canvas.GetTop(player1Chip);
            }
            else if (currentIndex == 1)
            {
                currentPlayer.CurrentPositionX = (int)Canvas.GetLeft(player2Chip);
                currentPlayer.CurrentPositionY = (int)Canvas.GetTop(player2Chip);
            }
            else if (currentIndex == 2)
            {
                currentPlayer.CurrentPositionX = (int)Canvas.GetLeft(player3Chip);
                currentPlayer.CurrentPositionY = (int)Canvas.GetTop(player3Chip);
            }
            else
            {
                currentPlayer.CurrentPositionX = (int)Canvas.GetLeft(player4Chip);
                currentPlayer.CurrentPositionY = (int)Canvas.GetTop(player4Chip);
            }
            
            CubeMessage cubeMessage = new CubeMessage(player.clientIp, currentPlayer.id, currentPlayer.LeftCube, currentPlayer.RightCube);
            player.SendCubeMessage(cubeMessage);
            
            //Cube1.Source = ImageSource(currentPlayer.LeftCube);
            //cube.LeftCube = RandomNumber;
            //Cube2.Source = ImageSource(currentPlayer.RightCube);
            //cube.RightCube = RandomNumber;
            //CubeSum = currentPlayer.LeftCube + currentPlayer.RightCube;

            //currentPlayer.LeftCube = cube.LeftCube;
            //currentPlayer.RightCube = cube.RightCube;

            //message = currentPlayer.Name + " выбрасывает " + cube.LeftCube.ToString() + ":" + cube.RightCube.ToString();

            //player.SendPublicMessage(message);

            //currentPlayer.movesLeft--;
            /*if (cube.RightCube == cube.LeftCube)
            {
                message = currentPlayer.Name + " выбрасывает " + cube.LeftCube.ToString() + ":" + cube.RightCube.ToString() + " и получает ещё один ход, так как выпал дубль";
                txtChat.AppendText(message + Environment.NewLine);
                txtChat.ScrollToEnd();

                currentPlayer.movesLeft++;
            }

            currentPlayer.CurrentFieldIndex = currentPlayer.FinalFieldIndex;
            currentPlayer.FinalFieldIndex = (currentPlayer.FinalFieldIndex + CubeSum) % 40;
            logic.Initialize(fields, currentPlayer, currentIndex);

            MovePlayerTo = fields.ListOfCoordinates[currentPlayer.FinalFieldIndex];
            if (currentIndex == 0) currentIndex = logic.MovePlayer(MovePlayerTo, player1Chip);
            else if (currentIndex == 1) currentIndex = logic.MovePlayer(MovePlayerTo, player2Chip);
            else if (currentIndex == 2) currentIndex = logic.MovePlayer(MovePlayerTo, player3Chip);
            else currentIndex = logic.MovePlayer(MovePlayerTo, player4Chip);

            ThrowCubesPanel.Visibility = Visibility.Hidden;
            Cube1.Visibility = Visibility.Visible;
            Cube2.Visibility = Visibility.Visible;*/
        }

        public void HideBuyFieldPanel()
        {
            BuyFieldPanel.Visibility = Visibility.Hidden;
        }

        public void HidePayFieldPanel()
        {
            PayFieldPanel.Visibility = Visibility.Hidden;
        }

        public bool NoMovesLeft()
        {
            if (currentPlayer.movesLeft <= 0)
            {
                MessageBox.Show(currentPlayer.name + " " + currentPlayer.movesLeft.ToString());
                return true;
            } 
            else return false;
        }

        private void RefuseButton_Click(object sender, RoutedEventArgs e)
        {
            //if (NoMovesLeft()) NextPlayer();
            HideBuyFieldPanel();
            ShowThrowCubesPanel();
        }

        private void GiveUpButton_Click(object sender, RoutedEventArgs e)
        {
            ListOfPlayers.Remove(currentPlayer);
            if (currentPlayer.id == 0) player1Chip.Visibility = Visibility.Hidden;
            else if (currentPlayer.id == 1) player2Chip.Visibility = Visibility.Hidden;
            else if (currentPlayer.id == 2) player3Chip.Visibility = Visibility.Hidden;
            else player4Chip.Visibility = Visibility.Hidden;

            if (ListOfPlayers.Count > 0)
            {
                //NextPlayer();
                HidePayFieldPanel();
                ShowThrowCubesPanel();
            }
        }

        public bool OneOfRadioButtonIsChecked()
        {
            if (((bool)one.IsChecked) || ((bool)two.IsChecked) || ((bool)three.IsChecked) 
                || ((bool)four.IsChecked) || ((bool)five.IsChecked) || ((bool)six.IsChecked))
            {
                return true;
            }
            else return false;
        }

        public int CheckedRadioButtonNumber()
        {
            if ((bool)one.IsChecked) return 1;
            else if ((bool)two.IsChecked) return 2;
            else if ((bool)three.IsChecked) return 3;
            else if ((bool)four.IsChecked) return 4;
            else if ((bool)five.IsChecked) return 5;
            else return 6;
        }

        public void HideCasinoPanel()
        {
            CasinoPanel.Visibility = Visibility.Hidden;
        }

        public void UncheckRadioButtons()
        {
            one.IsChecked = false;
            two.IsChecked = false;
            three.IsChecked = false;
            four.IsChecked = false;
            five.IsChecked = false;
            six.IsChecked = false;
        }

        private void HandleCasinoEvent()
        {
            Action showCube = delegate
            {
                const int PRICE = 1000;
                const int WIN_MONEY = 5000;

                int value = CheckedRadioButtonNumber();
                currentPlayer.Money -= PRICE;

                Cube1.Source = ImageSource(player.LeftCube);
                Cube1.Visibility = Visibility.Visible;

                string message = player.ListOfParticipants[currentPlayer.id].name + " выбрасывает число " + player.LeftCube.ToString();

                if (value == player.LeftCube)
                {
                    message += " и выигрывает свою ставку";
                    currentPlayer.Money += WIN_MONEY;
                    if (currentPlayer.id == player.Index) MessageBox.Show("Вы выйграли " + WIN_MONEY.ToString() + "k");
                }
                else
                {
                    message += " и проигрывает свою ставку";
                    if (currentPlayer.id == player.Index)  MessageBox.Show("Вы не угадали");
                }
                player.SendDescriptionMessage(message);

                UncheckRadioButtons();
                Cube1.Visibility = Visibility.Hidden;
                HideCasinoPanel();
                SendMoneyMessage();
            };
            Cube1.Dispatcher.Invoke(showCube);
        }

        private void CasinoPlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (OneOfRadioButtonIsChecked())
            {
                const int PRICE = 1000;
                const int WIN_MONEY = 5000;
                string message = "";

                int value = CheckedRadioButtonNumber();
                message = player.ListOfParticipants[currentPlayer.id].name + " ставит " + PRICE.ToString() + "k на число " + value.ToString() + " и бросает кубик...";
                player.SendDescriptionMessage(message);
                //txtChat.AppendText(message + Environment.NewLine);
                //txtChat.ScrollToEnd();
                CasinoMessage casinoMessage = new CasinoMessage(player.clientIp, currentPlayer.LeftCube);
                player.SendCasinoMessage(casinoMessage);
                /*
                CubeMessage cubeMessage = new CubeMessage(player.clientIp, currentPlayer.LeftCube, currentPlayer.RightCube);
                player.SendCubeMessage(cubeMessage);*/

                //Cube1.Source = ImageSource(currentPlayer.LeftCube);
                //cube.LeftCube = RandomNumber;
                //Cube1.Visibility = Visibility.Visible;

                //message = player.ListOfParticipants[currentPlayer.id].name + " выбрасывает число " + RandomNumber.ToString();
                /*
                if (value == RandomNumber)
                {
                    message += " и выигрывает свою ставку";
                    txtChat.AppendText(message + Environment.NewLine);
                    txtChat.ScrollToEnd();

                    currentPlayer.Money += WIN_MONEY;
                    MessageBox.Show("Вы выйграли " + WIN_MONEY.ToString() + "k");
                }
                else
                {
                    message += " и проигрывает свою ставку";
                    txtChat.AppendText(message + Environment.NewLine);
                    txtChat.ScrollToEnd();

                    MessageBox.Show("Вы не угадали");
                }*/
                //UncheckRadioButtons();
                //Cube1.Visibility = Visibility.Hidden;
                //UpdateAllPlayersMoney();

                //if (NoMovesLeft()) NextPlayer();
                //HideCasinoPanel();
                //ShowThrowCubesPanel();
            }
            else MessageBox.Show("Выберите значение, на которое хотите поставить");
        }

        private void RefuseToPlayCasino_Click(object sender, RoutedEventArgs e)
        {
            //if (NoMovesLeft()) NextPlayer();
            HideCasinoPanel();
            ShowThrowCubesPanel();
        }
    }
}
