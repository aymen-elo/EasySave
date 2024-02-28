using System;
using System.Threading;
using System.Windows;

namespace EasySaveGUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private Mutex mutex = new Mutex(true, "{12345678-1234-1234-1234-1234567890AB}");

        // an other instance is running
        protected override void OnStartup(StartupEventArgs e)
        {
            
            if (!mutex.WaitOne(TimeSpan.Zero, true))
            {
                MessageBox.Show("Une instance de l'application est déjà en cours d'exécution.");
                Current.Shutdown();
                return;
            }

            base.OnStartup(e);

        }

        protected override void OnExit(ExitEventArgs e)
        {
            mutex.ReleaseMutex();
            mutex.Dispose();

            base.OnExit(e);
        }
    }
}
