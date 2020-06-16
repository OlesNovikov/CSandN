using FileServiceLibrary;
using MessageClasses;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace ChatClient
{
    public partial class MainWindow : Window
    {
        private int receiverIndex = -1;
        private Client client;
        private FileClient fClient;
        private List<Participant> ListOfParticipants = new List<Participant>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ChatIsClosed(object sender, EventArgs e)
        {
            client.DisconnectClient();
        }

        public void ShowMessage(Message message)
        {
            Action ShowPrivateMessage = delegate
            {
                var privateMessage = message as PrivateMessage;

                string senderName = "";
                bool inDialog = false;

                if (ThisClientIsSender(privateMessage.senderId)) senderName = "You";
                else senderName = client.GetClientNameById(privateMessage.senderId);

                for (int i = 0; i < client.ListOfParticipants.Count; i++)
                {
                    if (ClientInDialogWithSender(i, privateMessage.senderId, privateMessage.receiverId))
                    {
                        ChatTextBox.AppendText(privateMessage.dateTime.ToShortTimeString() + " " + senderName + ": " + privateMessage.data + Environment.NewLine);
                        ChatTextBox.ScrollToEnd();
                        inDialog = true;
                    }
                }
                if ((!inDialog) && (receiverIndex < 1) && (client.id == privateMessage.receiverId)) NewPrivateMessagesTextBox.AppendText(privateMessage.dateTime.ToShortTimeString() + " New message from " + senderName + Environment.NewLine);
            };

            Action ShowPublicMessage = delegate
            {
                var publicMessage = message as PublicMessage;

                string senderName = "";
                string sendDateTime = publicMessage.dateTime.ToShortTimeString();

                if (ThisClientIsSender(publicMessage.ID)) senderName = "You";
                else senderName = client.GetClientNameById(publicMessage.ID);

                if (!DialogSelected())
                {
                    if (senderName == "") ChatTextBox.AppendText(sendDateTime + " " + publicMessage.data + Environment.NewLine);
                    else ChatTextBox.AppendText(sendDateTime + " " + senderName + ": " + publicMessage.data + Environment.NewLine);
                    ChatTextBox.ScrollToEnd();
                }
            };

            if (message is PublicMessage) ChatTextBox.Dispatcher.Invoke(ShowPublicMessage);
            else if (message is PrivateMessage) ChatTextBox.Dispatcher.Invoke(ShowPrivateMessage);
        }

        public void UpdateChatParticipants()
        {
            Action UpdateListOfParticipants = delegate
            {
                ParticipantsListView.Items.Clear();
                ParticipantsListView.Items.Add("Chat");
                foreach (var participant in client.ListOfParticipants)
                {
                    ParticipantsListView.Items.Add(participant.name);
                }
            };

            ParticipantsListView.Dispatcher.Invoke(UpdateListOfParticipants);
        }

        public bool DialogSelected()
        {
            if (receiverIndex > -1) return true;
            else return false;
        }

        private bool ThisClientIsSender(int senderId)
        {
            if (client.id == senderId) return true;
            else return false;
        }

        private bool ThisClientIsReceiver(int receiverId)
        {
            if (client.id == receiverId) return true;
            else return false;
        }

        private bool DialogWithReceiverIsActive(int receiverId)
        {
            if (client.ListOfParticipants[receiverIndex].id == receiverId) return true;
            else return false;
        }

        private bool DialogWithSenderIsActive(int senderId)
        {
            if (client.ListOfParticipants[receiverIndex].id == senderId) return true;
            else return false;
        }

        private bool ClientInDialogWithSender(int index, int senderId, int receiverId)
        {
            if ((index == receiverIndex) && ((ThisClientIsSender(senderId)) && (DialogWithReceiverIsActive(receiverId))
                                         || (ThisClientIsReceiver(receiverId)) && (DialogWithSenderIsActive(senderId))))
                return true;
            else return false;
        }

        public void ShowDialogHistory()
        {
            string senderName = "";
            ChatTextBox.Clear();
            
            foreach (var message in client.ListOfPrivateMessages)
            {
                if (ThisClientIsSender(message.senderId)) senderName = "You";
                else senderName = client.GetClientNameById(message.senderId);

                for (int i = 0; i < client.ListOfParticipants.Count; i++)
                {
                    if (ClientInDialogWithSender(i, message.senderId, message.receiverId))
                    {
                        ChatTextBox.AppendText(message.dateTime.ToShortTimeString() + " " + senderName + ": " + message.data + Environment.NewLine);
                        ChatTextBox.ScrollToEnd();
                    }
                }
            }
        }

        private void ParticipantsListView_Index(object sender, EventArgs e)
        {
            if ((receiverIndex >= client.ListOfParticipants.Count) || (receiverIndex < -1)) receiverIndex = -1;
            else receiverIndex = ParticipantsListView.SelectedIndex - 1;

            if (DialogSelected()) ShowDialogHistory();
            else SendHistoryRequest();
        }

        public void SendHistoryRequest()
        {
            HistoryRequestMessage historyRequest = new HistoryRequestMessage(client.ip, client.id);
            client.SendHistoryRequest(historyRequest);
        }

        private void AppLoaded(object sender, RoutedEventArgs e)
        {
            client = new Client();
            fClient = new FileClient();
            client.SendUdpRequest();

            client.MessageReceivedEvent += ShowMessage;
            client.ListOfParticipantsReceivedEvent += UpdateChatParticipants;
            client.HistoryMessageReceivedEvent += ShowMessagesHistory;
        }

        private bool MessageFromServer(string senderName, int id)
        {
            if ((senderName == "") && (id == -1)) return true;
            else return false;
        }

        private bool FromCurrentListOfParticipants(string senderName, int id)
        {
            if ((id > -1) && (senderName != "")) return true;
            else return false;
        }

        public void ShowMessagesHistory()
        {
            Action showHistory = delegate
            {
                string senderName = "";
                string sendDateTime = "";

                ChatTextBox.Clear();

                foreach (var message in client.ListOfPublicMessages)
                {
                    sendDateTime = message.dateTime.ToShortTimeString();
                    if (ThisClientIsSender(message.ID)) senderName = "You";
                    else senderName = client.GetClientNameById(message.ID);

                    if (MessageFromServer(senderName, message.ID)) ChatTextBox.AppendText(sendDateTime + " " + message.data + Environment.NewLine);
                    else if (FromCurrentListOfParticipants(senderName, message.ID)) ChatTextBox.AppendText(sendDateTime + " " + senderName + ": " + message.data + Environment.NewLine);
                    else
                    {
                        senderName = client.GetNameFromList(message.ID);
                        ChatTextBox.AppendText(sendDateTime + " " + senderName + ": " + message.data + Environment.NewLine);
                    }
                    ChatTextBox.ScrollToEnd();
                }
            };

            ChatTextBox.Dispatcher.Invoke(showHistory);
        }

        private void SendMessageButton_Click(object sender, RoutedEventArgs e)
        {
            string data = MessageTextBox.Text;
            if (!DialogSelected()) client.SendPublicMessage(data);
            else
            {
                for (int i = 0; i < client.ListOfParticipants.Count; i++)
                {
                    if (i == receiverIndex)
                    {
                        PrivateMessage message = new PrivateMessage(client.ip, DateTime.Now, client.id, data, client.ListOfParticipants[receiverIndex].id);
                        client.SendPrivateMessage(message);
                    }
                }
            }
            MessageTextBox.Text = null;
        }

        public void SetVisibilityProperties()
        {
            ConnectButton.Visibility = Visibility.Hidden;
            UsernameTextBox.Visibility = Visibility.Hidden;
            Username.Visibility = Visibility.Hidden;

            ChatTextBox.Visibility = Visibility.Visible;
            MessageTextBox.Visibility = Visibility.Visible;
            ParticipantsListView.Visibility = Visibility.Visible;
            SendMessageButton.Visibility = Visibility.Visible;
            Profile.Visibility = Visibility.Visible;
            ClientProfileName.Visibility = Visibility.Visible;
            NewPrivateMessagesTextBox.Visibility = Visibility.Visible;
            NewPrivateMessagesLabel.Visibility = Visibility.Visible;
            MarkEverythingAsReadLabel.Visibility = Visibility.Visible;
            MarkEverythingAsReadButton.Visibility = Visibility.Visible;
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (UsernameTextBox.Text.Length != 0)
                {
                    string clientName = UsernameTextBox.Text;

                    client.Name = clientName;
                    client.SendTcpRequest();

                    ClientProfileName.Content = client.Name;
                    SetVisibilityProperties();
                }
                else MessageBox.Show("Enter your name");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void MarkEverythingAsReadButton_Click(object sender, RoutedEventArgs e)
        {
            NewPrivateMessagesTextBox.Clear();
        }

        private async void LoadFileToServise_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            if (fileDialog.ShowDialog() == true)
            {
                string filePath = fileDialog.FileName;
                FileInfo selectedFile = new FileInfo(filePath);
                int fileSize = (int)selectedFile.Length;
                string fileExtension = selectedFile.Extension;

                if (fClient.SizeFits(fileSize) && fClient.ExtensionExists(fileExtension))
                {
                    int fileID = await fClient.LoadFileToService(filePath);
                    MessageBox.Show(fileID.ToString());
                }
                else MessageBox.Show("Chosen file size or extension does not fits");
            }
        }
    }
}