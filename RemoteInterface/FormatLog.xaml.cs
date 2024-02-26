using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows;
using EasySaveLib.Model;

namespace RemoteInterface
{
    public partial class FormatLog : Window
    {
        private Logger logger = Logger.GetInstance();

        public bool IsJsonSelected { get; private set; }
        public bool IsXmlSelected { get; private set; }
        public string encryptionKey { get; private set; }
        public string prioList { get; private set; }

        public FormatLog()
        {
            InitializeComponent();
            string logFormat = ConfigManager.GetLogFormat();
            if (logFormat == "xml")
            {
                rbXml.IsChecked = true;
            }
            else if (logFormat == "json")
            {
                rbJson.IsChecked = true;
            }
            
            tboxEncryptionKey.Text = ConfigManager.GetEncryptionKey();
            tboxCipherList.Text = ConfigManager.GetCipherList();
            tboxPrioList.Text = ConfigManager.GetPriorityList();
            tboxBigFile.Text = ConfigManager.GetBigFileSize();

        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (rbXml.IsChecked == true)
            {
                ConfigManager.SaveLogFormat("xml");
                logger.LogFormat = "xml";
                
            }
            else if (rbJson.IsChecked == true)
            {
                ConfigManager.SaveLogFormat("json");
                logger.LogFormat = "json";
            }

            string BigFileSize = tboxBigFile.Text;
            
            ConfigManager.SaveEncryptionKey(tboxEncryptionKey.Text);
            ConfigManager.SaveCipherList(tboxCipherList.Text);
            ConfigManager.SavePriorityList(tboxPrioList.Text);
            ConfigManager.SaveBigFileSize(tboxBigFile.Text);
            


            this.Close();
        }
    }
}
