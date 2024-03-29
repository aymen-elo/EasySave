﻿/* @Authors
 * Theo WATTIER
 * Perujan KOKILATHASAN
 * Aymen EL OUAGOUTI
 ******************************************
 * EasySave : Backup Management Application
 * (A3 FISA - 2023/2024 - CESI)
 */

using System;
using System.Threading;
using System.Windows;

namespace EasySaveGUI
{
    /// <summary>
    /// App General Configuration 
    /// </summary>
    public partial class App : Application
    {
        private Mutex mutex = new Mutex(true, "{12345678-1234-1234-1234-1234567890AB}");

        // an other instance is running
        protected override void OnStartup(StartupEventArgs e)
        {
            
            if (!mutex.WaitOne(TimeSpan.Zero, true))
            {
                MessageBox.Show("An instance of EasySave is already running.");
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
