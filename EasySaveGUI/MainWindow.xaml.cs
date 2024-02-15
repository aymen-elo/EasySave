using EasySave.Controllers;
using EasySave.Models;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EasySave_2._0
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();

        }

        private void btnNewJob_Click(object sender, RoutedEventArgs e) { }
        private void btnRunJob_Click(object sender, RoutedEventArgs e) { }
        private void btnRemoveJob_Click(object sender, RoutedEventArgs e) { }
        private void btnEditJob_Click(object sender, RoutedEventArgs e) { }
        private void btnPlayPause_Click(object sender, RoutedEventArgs e) { }
        private void btnStopJob_Click(object sender, RoutedEventArgs e) { }
        private void btnLogs_Click(object sender, RoutedEventArgs e) { }
    }
}
