using FileServiceLibrary;
using MessageClasses;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ChatClient
{
    public partial class MainWindow : Window
    {
        private const int SUCCESS_CODE = 200;
        private const int ERROR_CODE = 404;
        private int receiverIndex = -1;
        private int selectedFileIndex = -1;
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

        private  void ShowTextFileContent(string sendDateTime, string senderName, Message message, bool prvt, bool pblc)
        {
            string textContent = "";
            string fileContent = "";

            StackPanel messageStackPanel = new StackPanel();

            if (!pblc)
            {
                var privateMessage = message as PrivateMessage;

                if (senderName == "") textContent = sendDateTime + " " + privateMessage.data;
                else textContent = sendDateTime + " " + senderName + ": " + privateMessage.data;

                TextBox textMessage = new TextBox() { Text = textContent, TextWrapping = TextWrapping.Wrap, IsEnabled = false, IsReadOnly = true, BorderThickness = new Thickness(0) };
                messageStackPanel.Children.Add(textMessage);

                if (privateMessage.DictionaryOfFiles != null)
                {
                    foreach (var file in privateMessage.DictionaryOfFiles)
                    {
                        int fileSize = privateMessage.DictionaryOfSizes[file.Key];
                        fileContent = file.Value + " " + FileSize(fileSize);
                        int width = fileContent.Length * 6;
                        Button fileMessage = new Button() { Name = "ID" + file.Key.ToString(), Content = fileContent, Width = width };
                        fileMessage.Click += new RoutedEventHandler(DownloadFileButton_Click);
                        messageStackPanel.Children.Add(fileMessage);
                    }
                }
            }
            else
            {
                var publicMessage = message as PublicMessage;

                if (senderName == "") textContent = sendDateTime + " " + publicMessage.data;
                else textContent = sendDateTime + " " + senderName + ": " + publicMessage.data;

                TextBox textMessage = new TextBox() { Text = textContent, TextWrapping = TextWrapping.Wrap, IsEnabled = false, IsReadOnly = true, BorderThickness = new Thickness(0) };
                messageStackPanel.Children.Add(textMessage);

                if (publicMessage.DictionaryOfFiles != null)
                {
                    foreach (var file in publicMessage.DictionaryOfFiles)
                    {
                        int fileSize = publicMessage.DictionaryOfSizes[file.Key];
                        fileContent = file.Value + " " + FileSize(fileSize);
                        int width = fileContent.Length * 6;
                        Button fileMessage = new Button() { Name = "ID" + file.Key.ToString(), Content = fileContent, Width = width };
                        fileMessage.Click += new RoutedEventHandler(DownloadFileButton_Click);
                        messageStackPanel.Children.Add(fileMessage);
                    }
                }
            }            

            ParentStackPanel.Children.Add(messageStackPanel);
            ParentStackPanel.ScrollOwner.ScrollToEnd();
        }

        private int FileIDFromButton()
        {
            for (int i = 0; i < ParentStackPanel.Children.Count; i++)
            {
                var ChildStackPanel = (StackPanel)ParentStackPanel.Children[i];
                for (int j = 0; j < ChildStackPanel.Children.Count; j++)
                if (ChildStackPanel.Children[j].IsFocused)
                {
                    string stringFileID = ((Button)ChildStackPanel.Children[j]).Name;
                    int fileID = int.Parse(stringFileID.Substring(2));
                    return fileID;
                }
            }
            return ERROR_CODE;
        }

        private async void DownloadFileButton_Click(object sender, RoutedEventArgs e)
        {
            int fileID = FileIDFromButton();

            if (fileID != ERROR_CODE)
            {
                string downloadedFileName = await fClient.DownloadFileFromService(fileID);
                if (downloadedFileName != "")
                {
                    MessageBox.Show(downloadedFileName + " downloaded");
                }
                else MessageBox.Show("Download error");
            }
            else MessageBox.Show("Define fileID error");
        }

        public void ShowMessage(Message message)
        {
            Action ShowPrivateMessage = delegate
            {
                var privateMessage = message as PrivateMessage;

                string senderName = "";
                string sendDateTime = privateMessage.dateTime.ToShortTimeString();
                bool inDialog = false;

                if (ThisClientIsSender(privateMessage.senderId)) senderName = "You";
                else senderName = client.GetClientNameById(privateMessage.senderId);

                for (int i = 0; i < client.ListOfParticipants.Count; i++)
                {
                    if (ClientInDialogWithSender(i, privateMessage.senderId, privateMessage.receiverId))
                    {
                        ShowTextFileContent(sendDateTime, senderName, message, true, false);
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
                    ShowTextFileContent(sendDateTime, senderName, message, false, true);
                }
            };

            if (message is PublicMessage) ParentStackPanel.Dispatcher.Invoke(ShowPublicMessage);
            else if (message is PrivateMessage) ParentStackPanel.Dispatcher.Invoke(ShowPrivateMessage);
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

            ParentStackPanel.Children.Clear();

            foreach (var message in client.ListOfPrivateMessages)
            {
                if (ThisClientIsSender(message.senderId)) senderName = "You";
                else senderName = client.GetClientNameById(message.senderId);

                for (int i = 0; i < client.ListOfParticipants.Count; i++)
                {
                    if (ClientInDialogWithSender(i, message.senderId, message.receiverId))
                    {
                        string sendDateTime = message.dateTime.ToShortTimeString();
                        ShowTextFileContent(sendDateTime, senderName, message, true, false);
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

                ParentStackPanel.Children.Clear();

                foreach (var message in client.ListOfPublicMessages)
                {
                    sendDateTime = message.dateTime.ToShortTimeString();

                    if (ThisClientIsSender(message.ID)) senderName = "You";
                    else senderName = client.GetClientNameById(message.ID);

                    if (MessageFromServer(senderName, message.ID)) ShowTextFileContent(sendDateTime, senderName, message, false, true);
                    else if (FromCurrentListOfParticipants(senderName, message.ID)) ShowTextFileContent(sendDateTime, senderName, message, false, true);
                    else
                    {
                        senderName = client.GetNameFromList(message.ID);
                        ShowTextFileContent(sendDateTime, senderName, message, false, true);
                    }

                    ParentStackPanel.ScrollOwner.ScrollToEnd();
                }
            };

            ParentStackPanel.Dispatcher.Invoke(showHistory);
        }

        private void SendMessageButton_Click(object sender, RoutedEventArgs e)
        {
            string data = MessageTextBox.Text;
            if ((data != "") || (LoadedFilesComboBox.Items.Count != 0))
            {
                if (!DialogSelected()) client.SendPublicMessage(data, fClient.DictionaryOfFiles, client.DictionaryOfSizes);
                else
                {
                    for (int i = 0; i < client.ListOfParticipants.Count; i++)
                    {
                        if (i == receiverIndex)
                        {
                            PrivateMessage message = new PrivateMessage(client.ip, DateTime.Now, client.id, data, client.ListOfParticipants[receiverIndex].id, fClient.DictionaryOfFiles, client.DictionaryOfSizes);
                            client.SendPrivateMessage(message);
                        }
                    }
                }
                MessageTextBox.Text = null;
                fClient.DictionaryOfFiles.Clear();
                LoadedFilesComboBox.Items.Clear();
                fClient.TotalSize = 0;
                FilesSizeValueLabel.Content = FileSize(fClient.TotalSize);
            }
        }

        public void SetVisibilityProperties()
        {
            ConnectButton.Visibility = Visibility.Hidden;
            UsernameTextBox.Visibility = Visibility.Hidden;
            Username.Visibility = Visibility.Hidden;

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

        private string FileSize(float size)
        {
            const float KB = 1000.0f;
            const float MB = KB * KB;

            var fSize = size;
            string fSizeStr;

            if (fSize < KB)
            {
                fSizeStr = string.Format("{0:F2}", fSize);
                return fSizeStr + "B";
            }
            else if (fSize < MB)
            {
                fSize /= KB;
                fSizeStr = string.Format("{0:F2}", fSize);
                return fSizeStr + "KB";
            }
            else
            {
                fSize /= MB;
                fSizeStr = string.Format("{0:F2}", fSize);
                return fSizeStr + "MB";
            }
        }

        private void UpdateLoadedFilesDictionary()
        {
            LoadedFilesComboBox.Items.Clear();
            foreach (var file in fClient.DictionaryOfFiles)
            {
                LoadedFilesComboBox.Items.Add(file.Value);
            }
            var totalSize = (float)fClient.TotalSize;
            FilesSizeValueLabel.Content = FileSize(totalSize);
        }

        private async void LoadFileToService_Click(object sender, RoutedEventArgs e)
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
                    if (fileID != ERROR_CODE)
                    {
                        fileSize = await fClient.GetFileSize(fileID);
                        fClient.TotalSize += fileSize;
                        UpdateLoadedFilesDictionary();
                        MessageBox.Show("File loaded");
                    }
                    else MessageBox.Show("Load error");
                }
                else MessageBox.Show("Chosen file size or extension does not fits");
            }
        }

        private void LoadedFilesComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            selectedFileIndex = LoadedFilesComboBox.SelectedIndex;
        }

        private async void RemoveFileFromService_Click(object sender, RoutedEventArgs e)
        {
            if (selectedFileIndex > -1)
            {
                int removeID = SelectedFileID();
                int fileSize = await fClient.GetFileSize(removeID);
                int removeResult = await fClient.RemoveFileFromService(removeID);

                if (removeResult == SUCCESS_CODE)
                {
                    fClient.TotalSize -= fileSize;
                    UpdateLoadedFilesDictionary();
                    MessageBox.Show("File removed");
                }
                else MessageBox.Show("Remove error");
            }
            else MessageBox.Show("Chose file to remove");
        }

        private int SelectedFileID()
        {
            int i = 0;
            foreach (var file in fClient.DictionaryOfFiles)
            {
                if (i == selectedFileIndex) return file.Key;
                else i++;
            }
            return -1;
        }
    }
}