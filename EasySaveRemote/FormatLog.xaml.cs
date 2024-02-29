using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Windows;
using EasySaveLib.Model;
using EasySaveRemote.Packets;

namespace EasySaveRemote
{
    public partial class FormatLog : Window
    {
        public MainWindow _MainWindow { get; set; }



        public FormatLog()
        {
            InitializeComponent();
        }

        public FormatLog(string logFormat, string encryptionKey, string cipherList, string prioList, string bigFileSize, MainWindow mainWindow = null)
        {
            InitializeComponent();
            if (logFormat == "xml")
            {
                rbXml.IsChecked = true;
            }
            else if (logFormat == "json")
            {
                rbJson.IsChecked = true;
            }

            tboxEncryptionKey.Text = encryptionKey;
            tboxCipherList.Text = cipherList;
            tboxPrioList.Text = prioList;
            tboxBigFile.Text = bigFileSize;
            _MainWindow = mainWindow;

        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            string newConf = String.Empty;
            //newConf = _mainWindow.language;
            if (rbXml.IsChecked == true)
            {
                newConf = newConf + ";xml";
                
            }
            else if (rbJson.IsChecked == true)
            {
                newConf = newConf + ";json";
            }

            newConf = $"{newConf};{tboxEncryptionKey.Text};{tboxCipherList.Text};{tboxPrioList.Text};{tboxBigFile.Text}";
            SendMessage.SendMessageTo(_MainWindow.ipAddress, _MainWindow.port, newConf, MessageType.MO, _MainWindow );
            
            //Refresh Job & Option
            _MainWindow.refreshJO();

            
            this.Close();
        }
    }
}
