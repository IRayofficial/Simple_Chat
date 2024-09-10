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
        public MainWindow()
        {
            InitializeComponent();

            Exchanger = new HellmanKeyExchange();
            PrivateKey = Exchanger.initPrivateKey(encrypt.AlphabetNew());
            PublicKey = Exchanger.generatePublicKey(PrivateKey, encrypt.AlphabetNew());
        }

        private void calculateSharedKey(string partnerKey)
        {
            SharedKey = Exchanger.getSharedKey(partnerKey, PrivateKey, encrypt.AlphabetNew());
        }

        private void sendButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}