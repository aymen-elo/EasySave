using System;
using System.Windows;
using EasySaveLib.Model;

namespace EasySaveGUI
{
    public partial class Option : Window
    {
        private Logger logger = Logger.GetInstance();

        public bool IsJsonSelected { get; private set; }
        public bool IsXmlSelected { get; private set; }
        public string EncryptionKey { get; private set; }
        public string CipherList { get; private set; }
        public string PrioList { get; private set; }
        public string ProcessList { get; private set; }

        public Option()
        {
            InitializeComponent();
            try
            {
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
                tboxProcessList.Text = ConfigManager.GetProcessList();

                ConfigManager.SaveLogFormat("json");
                logger.LogFormat = "json";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while loading options: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
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
                ConfigManager.SaveProcessList(tboxProcessList.Text);

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while saving options: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
