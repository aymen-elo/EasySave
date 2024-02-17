using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using EasySave.Controllers;
using EasySave.Library;
using EasySave.Models;

namespace EasySave_2._0
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
                Logger.GetInstance()._logFormat = "json";
            }
            else if (rbXml.IsChecked == true)
            {
                // Enregistrer le choix Xml dans la classe Logger
                Logger.GetInstance()._logFormat = "xml";
            }

            // Fermer la fenêtre OptionsWindow
            this.Close();
        }
    }

    
}
