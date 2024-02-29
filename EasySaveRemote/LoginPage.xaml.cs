using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EasySaveRemote
{
    public partial class LoginPage : Window
    {
        public LoginPage()
        {
            InitializeComponent();
            tboxIpAddress.Text = "127.0.0.1";
            tboxPort.Text = "13";
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            string ipAddress = tboxIpAddress.Text;
            int port = Int32.Parse(tboxPort.Text);
            mainWindow.firstConnection(ipAddress, port);
            this.Close();
            mainWindow.ShowDialog();
        }
        
        
    }
}