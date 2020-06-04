using MessageClasses;
using System;
using System.Collections.Generic;
using System.Net;
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
        int currentIndex = 0;
        public AllFields fields;
        public List<Participant> ListOfPlayers = new List<Participant>();
        public StackPanel Profile;

        const string TextBoxHint = "Enter message";
        const string NicknameHint = "Nickname";
        const string ServerIpHint = "Server ip";
        const string whiteColor = "#ffffff";

        public MainWindow()
        {
            InitializeComponent();
        }

//------bool methods-----------------------------------------------------------------------------------------------------------------

        private bool FieldOnTopOrBottom(int index)
        {
            if ((index < 10) || ((index > 20) && (index < 30))) return true;
            else return false;
        }

        private bool InCasino()
        {
            int casino = 20;
            if (currentPlayer.FinalFieldIndex == casino) return true;
            else return false;
        }

        private bool OneOfRadioButtonIsChecked()
        {
            if (((bool)one.IsChecked) || ((bool)two.IsChecked) || ((bool)three.IsChecked)
                || ((bool)four.IsChecked) || ((bool)five.IsChecked) || ((bool)six.IsChecked))
            {
                return true;
            }
            else return false;
        }

        public bool NoMovesLeft()
        {
            if (currentPlayer.movesLeft <= 0)
            {
                return true;
            }
            else return false;
        }

        private bool PayTaxes()
        {
            const int TAX = 4;
            const int LUXURY_TAX = 36;

            if ((currentPlayer.FinalFieldIndex == TAX) || (currentPlayer.FinalFieldIndex == LUXURY_TAX)) return true;
            else return false;
        }

        private bool PayRent()
        {
            Field currentField = fields.ListOfFields[currentPlayer.FinalFieldIndex];
            if ((currentField.OwnerColor != currentPlayer.Color) && (currentField.OwnerColor != whiteColor)) return true;
            else return false;
        }

        private bool OnChanceField()
        {
            List<int> chanceFieldIndex = new List<int>() { 2, 7, 17, 22, 33, 38 };
            foreach (var field in chanceFieldIndex)
            {
                if (currentPlayer.FinalFieldIndex == field) return true;
            }
            return false;
        }

        private bool PlayerOnTopOrBottomLine()
        {
            if ((currentPlayer.FinalFieldIndex < 10)
            || ((currentPlayer.FinalFieldIndex > 20) && (currentPlayer.FinalFieldIndex < 30))) return true;
            else return false;
        }

        private bool StartField()
        {
            if (currentPlayer.FinalFieldIndex == 0) return true;
            else return false;
        }

//------Visualization methods--------------------------------------------------------------------------------------------------------

        private void HideChips()
        {
            player1Chip.Visibility = Visibility.Hidden;
            player2Chip.Visibility = Visibility.Hidden;
            player3Chip.Visibility = Visibility.Hidden;
            player4Chip.Visibility = Visibility.Hidden;
        }

        private void HideThrowCubesPanel()
        {
            ThrowCubesPanel.Visibility = Visibility.Hidden;
        }

        private void HideCasinoPanel()
        {
            CasinoPanel.Visibility = Visibility.Hidden;
        }

        private void HideBuyFieldPanel()
        {
            BuyFieldPanel.Visibility = Visibility.Hidden;
        }

        private void HidePayFieldPanel()
        {
            PayFieldPanel.Visibility = Visibility.Hidden;
        }

        private void UncheckRadioButtons()
        {
            one.IsChecked = false;
            two.IsChecked = false;
            three.IsChecked = false;
            four.IsChecked = false;
            five.IsChecked = false;
            six.IsChecked = false;
        }

        private SolidColorBrush BrushFromHex(string hexColorString)
        {
            return (SolidColorBrush)(new BrushConverter().ConvertFrom(hexColorString));
        }

        private void ShowMessageTextBoxHint()
        {
            string hintColor = "#656d78";
            MessageTextBox.Text = TextBoxHint;
            MessageTextBox.Foreground = BrushFromHex(hintColor);
        }

        private BitmapImage ImageSource(int randNumber)
        {
            string CubeSource = randNumber.ToString() + ".bmp";

            BitmapImage myCubeImage = new BitmapImage();
            myCubeImage.BeginInit();
            myCubeImage.UriSource = new Uri(@"Images/Coub/" + CubeSource, UriKind.RelativeOrAbsolute);
            myCubeImage.EndInit();
            return myCubeImage;
        }

        //------Initialization methods-------------------------------------------------------------------------------------------------------

        private void InitializeFields()
        {
            fields = new AllFields();
            for (int index = 0; index < fields.ListOfFields.Count; index++)
            {
                Field field = fields.ListOfFields[index];
                if (field.Price != 0)
                {
                    if (FieldOnTopOrBottom(index)) ((TextBlock)FindName(field.Name)).Text = field.Price.ToString() + "k";
                    else ((TabItem)FindName(field.Name)).Header = "  " + field.Price.ToString() + "k";
                }
            }
        }

        private void AddEventsToPlayer()
        {
            player.ListOfParticipantsReceivedEvent += ShowAllProfiles;
            player.StartGameMessageReceivedEvent += ShowThrowCubesPanel;
            player.PublicMessageReceivedEvent += ReceivePublicMessage;
            player.DescriptionMessageReceivedEvent += ReceiveDescriptionMessage;
            player.CubeMessageReceivedEvent += ShowCubesAndMove;
            player.MoneyMessageReceivedEvent += UpdateMoney;
            player.MoveMessageReceivedEvent += SetupCurrentPlayer;
            player.CasinoMessageReceivedEvent += HandleCasinoEvent;
            player.DisconnectMessageReceivedEvent += RedrawFieldsAndPlayers;
        }

        private void InitializeNewPlayer()
        {
            player.Name = NicknameTextBox.Text;
            player.Money = 15000;
            player.CurrentFieldIndex = 0;
            player.FinalFieldIndex = 0;
            player.movesLeft = 1;
        }

//------Main Window methods----------------------------------------------------------------------------------------------------------

        private void MainWindowLoaded(object sender, RoutedEventArgs e)
        {
            player = new PlayerData(this);
            AddEventsToPlayer();
            player.SendUdpRequest();

            InitializeFields();
            HideChips();
            MessageTextBox.Text = TextBoxHint;
            NicknameTextBox.Text = NicknameHint;
            ServerIpTextBox.Text = ServerIpHint;
        }

        private void MainWindowClosed(object sender, EventArgs e)
        {
            if (player.tcpListenSocket != null)
            {
                int removeIndex = 0;
                for (int i = 0; i < player.ListOfParticipants.Count; i++)
                {
                    if (player.ListOfParticipants[i].id == player.Index)
                    {
                        removeIndex = i;
                    }
                }
                if ((player.ListOfParticipants.Count > 0) && (currentPlayer != null))
                {
                    player.ListOfParticipants.RemoveAt(removeIndex);
                    DisconnectMessage disconnectMessage = new DisconnectMessage(player.clientIp, player.ListOfParticipants, player.Index, player.Color, currentPlayer.id);
                    player.SendDisconnectMessage(disconnectMessage);
                }
                player.DisconnectClient();
            }
        }

        private void NicknameTextBox_Enter(object sender, RoutedEventArgs e)
        {
            if ((NicknameTextBox.Text.Length == 0) || (NicknameTextBox.Text == NicknameHint))
            {
                NicknameTextBox.Text = null;
            }
        }

        private void MessageTextBox_Enter(object sender, RoutedEventArgs e)
        {
            if ((MessageTextBox.Text.Length == 0) || (MessageTextBox.Text == TextBoxHint))
            {
                MessageTextBox.Text = null;
                MessageTextBox.Foreground = BrushFromHex(whiteColor);
            }
        }

        private void ServerIpTextBox_Enter(object sender, RoutedEventArgs e)
        {
            if ((ServerIpTextBox.Text.Length == 0) || (ServerIpTextBox.Text == ServerIpHint))
            {
                ServerIpTextBox.Text = null;
            }
        }

        private void NicknameTextBox_Leave(object sender, RoutedEventArgs e)
        {
            if (NicknameTextBox.Text.Length == 0)
            {
                NicknameTextBox.Text = NicknameHint;
            }
        }

        private void MessageTextBox_Leave(object sender, RoutedEventArgs e)
        {
            if (MessageTextBox.Text.Length == 0)
            {
                ShowMessageTextBoxHint();
            }
        }

        private void ServerIpTextBox_Leave(object sender, RoutedEventArgs e)
        {
            if (ServerIpTextBox.Text.Length == 0)
            {
                ServerIpTextBox.Text = ServerIpHint;
            }
        }

        private void SendMessage_Click(object sender, RoutedEventArgs e)
        {
            if (MessageTextBox.Text != TextBoxHint)
            {
                string message = MessageTextBox.Text;
                player.SendPublicMessage(message);
                ShowMessageTextBoxHint();
            }
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            logic = new MovesLogic(this);
            InitializeNewPlayer();
            if ((bool)ServerIpRadioButton.IsChecked)
            {
                player.serverIpPort.Address = IPAddress.Parse(ServerIpTextBox.Text);
                ConnectButton.IsEnabled = false;
                if (player.Name != null) player.SendTcpRequest();
            }
            else if ((bool)LocalIpRadioButton.IsChecked)
            {
                ConnectButton.IsEnabled = false;
                if (player.Name != null) player.SendTcpRequest();
            }
            else MessageBox.Show("Выберите способ подключения");
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
        }

        private void PayFieldButton_Click(object sender, RoutedEventArgs e)
        {
            string message = "";
            int moneyToPay = 0;
            Field currentField = fields.ListOfFields[currentPlayer.FinalFieldIndex];

            moneyToPay = currentField.CurrentRent;

            currentPlayer.Money -= moneyToPay;

            if (!PayTaxes() && !OnChanceField() && PayRent()) PayToAnotherPlayer(moneyToPay, currentField);

            message = currentPlayer.name + " оплачивает расходы";
            player.SendDescriptionMessage(message);

            HidePayFieldPanel();
            SendMoneyMessage();
        }

        private void ThrowCubesButton_Click(object sender, RoutedEventArgs e)

        {
            currentPlayer = player.ListOfParticipants[currentIndex];

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
        }

        private void RefuseButton_Click(object sender, RoutedEventArgs e)
        {
            HideBuyFieldPanel();
            if (currentPlayer.movesLeft == 0) NextPlayer();
            else if (currentPlayer.id == player.Index) ShowThrowCubesPanel();
        }

        private void GiveUpButton_Click(object sender, RoutedEventArgs e)
        {
            if (player.tcpListenSocket != null)
            {
                int removeIndex = 0;
                for (int i = 0; i < player.ListOfParticipants.Count; i++)
                {
                    if (player.ListOfParticipants[i].id == player.Index)
                    {
                        removeIndex = i;
                    }
                }
                if ((player.ListOfParticipants.Count > 0) && (currentPlayer != null))
                {
                    player.ListOfParticipants.RemoveAt(removeIndex);
                    DisconnectMessage disconnectMessage = new DisconnectMessage(player.clientIp, player.ListOfParticipants, player.Index, player.Color, currentPlayer.id);
                    player.SendDisconnectMessage(disconnectMessage);
                }
                player.DisconnectClient();
            }
        }

        private void CasinoPlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (OneOfRadioButtonIsChecked())
            {
                const int PRICE = 1000;
                string message = "";

                int value = CheckedRadioButtonNumber();
                message = player.ListOfParticipants[currentPlayer.id].name + " ставит " + PRICE.ToString() + "k на число " + value.ToString() + " и бросает кубик...";
                player.SendDescriptionMessage(message);
                CasinoMessage casinoMessage = new CasinoMessage(player.clientIp, currentPlayer.LeftCube);
                player.SendCasinoMessage(casinoMessage);
            }
            else MessageBox.Show("Выберите значение, на которое хотите поставить");
        }

        private void RefuseToPlayCasino_Click(object sender, RoutedEventArgs e)
        {
            HideCasinoPanel();
            if (currentPlayer.movesLeft == 0) NextPlayer();
            else if (currentPlayer.id == player.Index) ShowThrowCubesPanel();
        }

//------Received messages delegates--------------------------------------------------------------------------------------------------

        private void RedrawFieldsAndPlayers(DisconnectMessage message)
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
                            if (FieldOnTopOrBottom(i))
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

                if (message.ListOfParticipants.Count == 1)
                {
                    WinLabel.Visibility = Visibility.Visible;
                    CanvasMap.Visibility = Visibility.Hidden;
                }
                else
                {
                    if (message.id == message.currentPlayerId)
                    {
                        if (message.id == player.ListOfParticipants.Count)
                        {
                            currentIndex = 0;
                            currentPlayer = player.ListOfParticipants[currentIndex];
                        }
                        else
                        {
                            currentIndex = message.currentPlayerId;
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
                }

                for (int i = 0; i < AllPlayersProfiles.Children.Count; i++)
                {
                    var stackPanel = (StackPanel)AllPlayersProfiles.Children[i];
                    stackPanel.Background = BrushFromHex("#161a1b");
                }

                StackPanel panel = (StackPanel)AllPlayersProfiles.Children[currentPlayer.id];
                panel.Background = BrushFromHex(currentPlayer.Color);
                if (message.currentPlayerId == player.Index)
                {
                    ShowThrowCubesPanel();
                }
            };

            player1Chip.Dispatcher.Invoke(redraw);
        }

        private void ShowAllProfiles()
        {
            Action showProfiles = delegate
            {
                AllPlayersProfiles.Children.Clear();
                foreach (var participant in player.ListOfParticipants)
                {
                    if (player.Index == participant.id) player.Color = participant.id.ToString();
                    if (participant.id == 0) Profile = new StackPanel { Name = "Player" + participant.id.ToString(), Width = 160, Height = 120, Margin = new Thickness(0, 0, 0, 0), Background = BrushFromHex("#161a1b") };
                    else Profile = new StackPanel { Name = "Player" + participant.id.ToString(), Width = 160, Height = 120, Margin = new Thickness(0, 30, 0, 0), Background = BrushFromHex("#161a1b") };

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

        private void SetupCurrentPlayer(MoveMessage message)
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
                if (message.id == player.Index)
                {
                    ShowThrowCubesPanel();
                }
            };

            ThrowCubesPanel.Dispatcher.Invoke(next);
        }

        private void ShowPublicMessage(string message)
        {
            Action showMessage = delegate
            {
                txtChat.AppendText(message + Environment.NewLine);
                txtChat.ScrollToEnd();
            };
            txtChat.Dispatcher.Invoke(showMessage);
        }

        private void ShowCubesAndMove()
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

                message = currentPlayer.name + " выбрасывает " + currentPlayer.LeftCube.ToString() + ":" + currentPlayer.RightCube.ToString();

                currentPlayer.movesLeft--;

                if (currentPlayer.RightCube == currentPlayer.LeftCube)
                {
                    if (player.Index == currentPlayer.id)
                    {
                        message += ". " + currentPlayer.name + " выбрасывает " + currentPlayer.LeftCube.ToString() + ":" + currentPlayer.RightCube.ToString() + " и получает ещё один ход, так как выпал дубль";
                    }
                    currentPlayer.movesLeft++;
                }
                if (player.Index == currentPlayer.id)
                {
                    player.SendDescriptionMessage(message);
                }

                currentPlayer.CurrentFieldIndex = currentPlayer.FinalFieldIndex;
                currentPlayer.FinalFieldIndex = (currentPlayer.FinalFieldIndex + cubeSum) % 40;

                logic.Add(fields.ListOfFields, player.ListOfParticipants, player);
                logic.Initialize(fields, currentPlayer, currentIndex);

                Point MovePlayerTo = fields.ListOfCoordinates[currentPlayer.FinalFieldIndex];

                if (currentIndex == 0) logic.MovePlayer(MovePlayerTo, player1Chip);
                else if (currentIndex == 1) logic.MovePlayer(MovePlayerTo, player2Chip);
                else if (currentIndex == 2) logic.MovePlayer(MovePlayerTo, player3Chip);
                else logic.MovePlayer(MovePlayerTo, player4Chip);
            };
            Dispatcher.Invoke(showCubes);
        }

        private void UpdateMoney()
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

                if (!PayTaxes() && !OnChanceField() && !PayRent() && !InCasino() && !StartField()) ProcessPurchase(componentName, fieldToBuy);

                if (player.Index == currentPlayer.id)
                {
                    if (NoMovesLeft()) NextPlayer();
                    else ShowThrowCubesPanel();
                }
            };
            AllPlayersProfiles.Dispatcher.Invoke(updateMoney);
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

                    for (int i = 0; i < player.ListOfParticipants.Count; i++)
                    {
                        if (currentPlayer.id == player.ListOfParticipants[i].id) player.ListOfParticipants[i].Money += WIN_MONEY;
                    }
                    if (currentPlayer.id == player.Index) MessageBox.Show("Вы выйграли " + WIN_MONEY.ToString() + "k");
                }
                else
                {
                    message += " и проигрывает свою ставку";
                    if (currentPlayer.id == player.Index) MessageBox.Show("Вы не угадали");
                }
                if (currentPlayer.id == player.Index) player.SendDescriptionMessage(message);

                UncheckRadioButtons();
                Cube1.Visibility = Visibility.Hidden;
                HideCasinoPanel();
                SendMoneyMessage();
            };
            Cube1.Dispatcher.Invoke(showCube);
        }

//------other methods----------------------------------------------------------------------------------------------------------------

        private void ReceivePublicMessage(PublicMessage publicMessage)
        {
            string message = publicMessage.senderName + ": " + publicMessage.data;
            ShowPublicMessage(message);
        }

        private void ReceiveDescriptionMessage(DescriptionMessage descriptionMessage)
        {
            string message = descriptionMessage.data;
            ShowPublicMessage(message);
        }

        private void ProcessPurchase(string componentName, Field fieldToBuy)
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

        public void SendMoneyMessage()
        {
            MoneyMessage moneyMessage = new MoneyMessage(player.clientIp, player.ListOfParticipants, DateTime.Now);
            player.SendMoneyMessage(moneyMessage);
        }

        private void PayToAnotherPlayer(int moneyToPay, Field currentField)
        {
            foreach (var fieldOwner in player.ListOfParticipants)
            {
                if (fieldOwner.Color == currentField.OwnerColor)
                {
                    fieldOwner.Money += moneyToPay;
                }
            }
        }

        public void NextPlayer()
        {
            if (currentPlayer.movesLeft == 0) player.ListOfParticipants[currentIndex].movesLeft++;
            currentIndex++;
            currentIndex %= player.ListOfParticipants.Count;
            currentPlayer = player.ListOfParticipants[currentIndex];
            player.SendNextPlayerMoveMessage(currentPlayer);
        }

        private int CheckedRadioButtonNumber()
        {
            if ((bool)one.IsChecked) return 1;
            else if ((bool)two.IsChecked) return 2;
            else if ((bool)three.IsChecked) return 3;
            else if ((bool)four.IsChecked) return 4;
            else if ((bool)five.IsChecked) return 5;
            else return 6;
        }
    }
}