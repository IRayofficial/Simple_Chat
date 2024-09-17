using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json;
using SimpleEncrypt;

namespace Simple_Chat
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string MyUserName;
        static EncryptManager encryptManager = new EncryptManager(16);
        public string PublicKey = encryptManager.GetPublicKey();
        public bool connected = false;

        public string RecievedContentPattern = @"<type>(.*?)</type>|<content>(.*?)</content>";
        public string GetMessage = @"<username>(.*?)</username>|<message>(.*?)</message>";
        public string GetUserInfo = @"<username>(.*?)</username>|<public-key>(.*?)</public-key>";
        public string GetClient = @"<client>(.*?)</client>";

        private List<User> userList = new List<User>();
        private User selectedUser = null;

        NetworkStream stream;
        public MainWindow()
        {
            InitializeComponent();        
        }

        //Connections Click
        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(UserName.Text)) {
                MyUserName = UserName.Text;
                Task.Run(() => connectToServer());
            }
            else { ConnectError.Text = "Username cannot be empty"; }
        }

        private async Task connectToServer()
        {
            try
            {
                /* to test the software just use the chat server from my Github repostiory 
                 * https://github.com/IRayofficial/Chat_Server.git
                 * and run it localy or run it on a Server but if you do change the adress to the internal or external
                 * server adress in the config.json
                */
                Config config;
                using (StreamReader reader = new StreamReader("config.json"))
                {
                    string json = reader.ReadToEnd();
                    config = JsonConvert.DeserializeObject<Config>(json);
                }
                TcpClient client = new TcpClient(config.ServerAddress, config.Port);
                stream = client.GetStream();

                string connectionMessage = "<username>" + MyUserName + "</username><public-key>" + PublicKey + "</public-key>";
                byte[] connectionMessageBytes = Encoding.UTF8.GetBytes(connectionMessage);
                stream.Write(connectionMessageBytes, 0, connectionMessageBytes.Length);

                connected = true;
                
                Dispatcher.Invoke(() =>
                {
                    LoginView.Visibility = Visibility.Hidden;
                    ChatView.Visibility = Visibility.Visible;
                });
                Task.Run(() => ReceiveMessages(stream));
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    ConnectError.Text = "Could not connect to Server";
                });
            }
        }

        //Event Listener for Clients
        private async Task ReceiveMessages(NetworkStream stream)
        {
            byte[] buffer = new byte[1024];
            int bytesRead;
            try
            {
                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    var matches = Regex.Matches(receivedMessage, RecievedContentPattern);
                    string type = "";
                    string content = "";
                    foreach (Match match in matches)
                    {
                        if (match.Groups[1].Success) { type = match.Groups[1].Value; }
                        if (match.Groups[2].Success) { content = match.Groups[2].Value; }
                    }

                    if (type == "USERLIST")
                    {
                        UpdateUserList(content);
                    }
                    else if (type == "MESSAGE")
                    {
                        string message = "";
                        string user = "";
                        var getMessage = Regex.Matches(content, GetMessage);
                        foreach (Match match in getMessage)
                        {
                            if (match.Groups[1].Success) { user = match.Groups[1].Value; }
                            if (match.Groups[2].Success) { message = match.Groups[2].Value; }
                        }

                        message = encryptManager.DecryptMessage(message);
                        Dispatcher.Invoke(() =>
                        {
                            AddMessageToStackPanel(user, message, Brushes.LightGreen, HorizontalAlignment.Left);
                        });
                    }
                }
            }
            catch (Exception ex) { connected = false; }
        }

        //Updates the User List
        private void UpdateUserList(string userListMessage)
        {
            List<string> users = new List<string>();
            var matches = Regex.Matches(userListMessage, GetClient);

            foreach (Match match in matches)
            {
                if (match.Groups[1].Success) { users.Add(match.Groups[1].Value); }
            }

            if (users.Count > 0) { userList.Clear(); }

            foreach (var user in users)
            {
                if (!string.IsNullOrEmpty(user))
                {
                    var userInfo = Regex.Matches(user, GetUserInfo);
                    string userName = "";
                    string publicKey = "";
                    foreach (Match match in userInfo)
                    {
                        if (match.Groups[1].Success) { userName = match.Groups[1].Value; }
                        if (match.Groups[2].Success) { publicKey = match.Groups[2].Value; }
                    }
                    User newUser = new User(userName, publicKey);
                    userList.Add(newUser);
                }
            }

            Dispatcher.Invoke(() =>
            {
                UserListPanel.Children.Clear();
                foreach (var user in userList)
                {
                    if (user.PublicKey != PublicKey)
                    {
                        UserListPanel.Children.Add(UserBox(user));
                    }
                }
            });
        }

        private UIElement UserBox(User user)
        {
            Border userBox = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CCCCCC")),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(10),
                Margin = new Thickness(5),
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1),
            };

            StackPanel infoPanel = new StackPanel();

            TextBlock usernameBlock = new TextBlock
            {
                Text = user.UserName,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.DarkBlue,
                Margin = new Thickness(0, 0, 0, 5)
            };

            TextBlock publicKeyBlock = new TextBlock
            {
                Text = user.PublicKey,
            };

            infoPanel.Children.Add(usernameBlock);
            infoPanel.Children.Add(publicKeyBlock);

            userBox.MouseDown += (s, e) => SelectUserForPrivateChat(user);
            userBox.Child = infoPanel;
            return userBox;
        }

        //Select User click
        private void SelectUserForPrivateChat(User user)
        {
            selectedUser = user;
            encryptManager.InitSharedKey(selectedUser.PublicKey);
            SelectedUserLabel.Text = MyUserName +" is chatting with: " + user.UserName;
        }

        //Send A Message
        private void sendButton_Click(object sender, RoutedEventArgs e)
        {
            string messageText = MessageInput.Text;

            if (!string.IsNullOrEmpty(messageText) && selectedUser != null && userList.Contains(selectedUser))
            {
                sendMessage(messageText);
            }
        }

        private void sendMessage(string messageText)
        {
            int userId = userList.IndexOf(selectedUser);
            encryptManager.InitSharedKey(selectedUser.PublicKey);
            messageText = encryptManager.EncryptMessage(messageText);
            string sendString = "<from>" + MyUserName + "</from><content>" + messageText + "</content><send-to>" + userId + "</send-to>";

            byte[] buffer = Encoding.UTF8.GetBytes(sendString);
            try
            {
                stream.Write(buffer, 0, buffer.Length);
                messageText = encryptManager.DecryptMessage(messageText);
                AddMessageToStackPanel(MyUserName, messageText, Brushes.LightBlue, HorizontalAlignment.Right);

                MessageInput.Text = string.Empty;
                ChatScrollViewer.ScrollToEnd();
            }
            catch (Exception ex) { connected = false; }
        }

        //Add message to UI
        private void AddMessageToStackPanel(string username, string message, Brush backgroundColor, HorizontalAlignment alignment)
        {
            Border messageBubble = new Border
            {
                Background = backgroundColor,
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(10),
                Margin = new Thickness(5),
                MaxWidth = 500, 
                HorizontalAlignment = alignment
            };

            StackPanel messagePanel = new StackPanel();

            TextBlock usernameBlock = new TextBlock
            {
                Text = username,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.DarkBlue,
                Margin = new Thickness(0, 0, 0, 5)
            };

            TextBlock messageBlock = new TextBlock
            {
                Text = message,
                TextWrapping = TextWrapping.Wrap
            };

            messagePanel.Children.Add(usernameBlock);
            messagePanel.Children.Add(messageBlock);

            messageBubble.Child = messagePanel;
            ChatPanel.Children.Add(messageBubble);
        }
    }
}