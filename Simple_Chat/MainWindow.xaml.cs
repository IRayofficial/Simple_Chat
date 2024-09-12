using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Simple_Chat
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static int[] PrivateKey;
        public string PublicKey;
        public string PartnerPublicKey;
        public string SharedKey;

        HellmanKeyExchange Exchanger;
        Encryption encrypt = new Encryption();
        NetworkStream stream;
        public MainWindow()
        {
            InitializeComponent();

            Exchanger = new HellmanKeyExchange();
            PrivateKey = Exchanger.initPrivateKey(encrypt.AlphabetNew());
            PublicKey = Exchanger.generatePublicKey(PrivateKey, encrypt.AlphabetNew());
            bool not_connected = true;
            while (not_connected)
            {
                try
                {
                    /* to test the software just use the chat server from my Github repostiory 
                     * https://github.com/IRayofficial/Chat_Server.git
                     * and run it localy or run it on a Server but if you do change the adress to the internal or external
                     * server adress
                     */
                    TcpClient client = new TcpClient("localhost", 8888); 
                    stream = client.GetStream();

                    Task.Run(() => ReceiveMessages(stream));
                    not_connected = false;
                }
                catch (Exception ex) { }
            }
            
        }

        //Ecryption methods
        private void calculateSharedKey(string partnerKey)
        {
            SharedKey = Exchanger.getSharedKey(partnerKey, PrivateKey, encrypt.AlphabetNew());
        }

        private string encryptMessage(string message)
        {
            return new string(encrypt.encodeKey(message, SharedKey));
        }

        private string decryptMessage(string message)
        {
            return new string(encrypt.decodeKey(message, SharedKey));
        }

        //Send A Message
        private void sendButton_Click(object sender, RoutedEventArgs e)
        {
            string messageText = MessageInput.Text;

            if (!string.IsNullOrEmpty(messageText))
            {
                byte[] buffer = Encoding.ASCII.GetBytes(messageText);
                stream.Write(buffer, 0, buffer.Length);

                //This Client
                AddMessageToStackPanel("User1", messageText, Brushes.LightBlue, HorizontalAlignment.Right);

                MessageInput.Text = string.Empty;
                ScrollToBottom();
            }
        }

        //Event Listener for Clients
        private void ReceiveMessages(NetworkStream stream)
        {
            byte[] buffer = new byte[1024];
            int bytesRead;

            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    AddMessageToStackPanel("User2", message, Brushes.LightGreen, HorizontalAlignment.Left);
                }));
            }
        }

        //Add to Chat UI
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

        //Scroll to end
        private void ScrollToBottom()
        {
            ChatScrollViewer.ScrollToEnd();
        }
    }
}