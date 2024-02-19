using System.Windows;
using EasySaveGUI.Model;

namespace EasySaveGUI
{
    public partial class OptionWindow : Window
    {
        // Propriétés pour stocker le choix de l'utilisateur
        public bool IsJsonSelected { get; private set; }
        public bool IsXmlSelected { get; private set; }

        public OptionWindow()
        {
            InitializeComponent();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (rbJson.IsChecked == true)
            {
                // Enregistrer le choix Json dans la classe Logger
                Logger.GetInstance().LogFormat = "json";
            }
            else if (rbXml.IsChecked == true)
            {
                // Enregistrer le choix Xml dans la classe Logger
                Logger.GetInstance().LogFormat = "xml";
            }

            // Fermer la fenêtre OptionsWindow
            this.Close();
        }
    }

    
}
