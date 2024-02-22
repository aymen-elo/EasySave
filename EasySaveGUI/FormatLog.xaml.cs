using System.Windows;
using EasySaveLib.Model;

namespace EasySaveGUI
{
    public partial class FormatLog : Window
    {
        private Logger logger = Logger.GetInstance();

        public bool IsJsonSelected { get; private set; }
        public bool IsXmlSelected { get; private set; }

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

            this.Close();
        }
    }
}
