using System;
using System.Windows;
using EasySaveLib.Model;

namespace EasySaveGUI.View
{
    /// <summary>
    /// Window for managing application options.
    /// </summary>
    public partial class OptionWindow : Window
    {
        /// <summary>
        /// Logger instance for logging application events.
        /// </summary>
        private readonly Logger _logger = Logger.GetInstance();

        public bool IsJsonSelected { get; private set; }
        public bool IsXmlSelected { get; private set; }
        public string EncryptionKey { get; private set; }
        public string CipherList { get; private set; }
        public string PrioList { get; private set; }
        public string ProcessList { get; private set; }

        /// <summary>
        /// Constructor for OptionWindow.
        /// Initializes the window and loads the current options.
        /// </summary>
        public OptionWindow()
        {
            InitializeComponent();
            try
            {
                var logFormat = ConfigManager.GetLogFormat();
                if (logFormat == "xml")
                    rbXml.IsChecked = true;
                else if (logFormat == "json") rbJson.IsChecked = true;

                tboxEncryptionKey.Text = ConfigManager.GetEncryptionKey();
                tboxCipherList.Text = ConfigManager.GetCipherList();
                tboxPrioList.Text = ConfigManager.GetPriorityList();
                tboxBigFile.Text = ConfigManager.GetBigFileSize();
                tboxProcessList.Text = ConfigManager.GetProcessList();

                ConfigManager.SaveLogFormat("json");
                _logger.LogFormat = "json";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while loading options: {ex.Message}", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Event handler for the save button click event.
        /// Saves the current options and closes the window.
        /// </summary>
        /// <param name="sender">The object that triggered the event.</param>
        /// <param name="e">The event arguments.</param>
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (rbXml.IsChecked == true)
                {
                    ConfigManager.SaveLogFormat("xml");
                    _logger.LogFormat = "xml";
                }
                else if (rbJson.IsChecked == true)
                {
                    ConfigManager.SaveLogFormat("json");
                    _logger.LogFormat = "json";
                }

                var bigFileSize = tboxBigFile.Text;

                ConfigManager.SaveEncryptionKey(tboxEncryptionKey.Text);
                ConfigManager.SaveCipherList(tboxCipherList.Text);
                ConfigManager.SavePriorityList(tboxPrioList.Text);
                ConfigManager.SaveBigFileSize(tboxBigFile.Text);
                ConfigManager.SaveProcessList(tboxProcessList.Text);

                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while saving options: {ex.Message}", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}