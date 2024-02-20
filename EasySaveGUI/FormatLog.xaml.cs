using System.Windows;
using EasySaveGUI.Model;

namespace EasySaveGUI
{
    public partial class FormatLog : Window
    {
        // Propriétés pour stocker le choix de l'utilisateur
        public bool IsJsonSelected { get; private set; }
        public bool IsXmlSelected { get; private set; }

        public FormatLog()
        {
            InitializeComponent();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (rbXml.IsChecked == true)
            {
                Logger.GetInstance().LogFormat = "xml";
                ConfigManager.SaveLogFormat("xml");
            }
            else if (rbJson.IsChecked == true)
            {
                Logger.GetInstance().LogFormat = "json";
                ConfigManager.SaveLogFormat("json");
            }

            this.Close();
        }
    }
}
