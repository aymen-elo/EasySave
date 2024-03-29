﻿/* @Authors
 * Theo WATTIER
 * Perujan KOKILATHASAN
 * Aymen EL OUAGOUTI
 ******************************************
 * EasySave : Backup Management Application
 * (A3 FISA - 2023/2024 - CESI)
 */

using EasySave.Model;
using EasySave.Controller;
using EasySave.Model.Translation;

namespace EasySave
{
    class Program
    {
        
        // The directory where the log files & hashes will be saved
        public static string LogsDirectoryPath = @"C:\Prosoft\EasySave\Logs";
        static void Main(string[] args)
        {
            // Initalization of the main components
            var logger = Logger.GetInstance();
            var jobsController = new JobsController(logger);
            var translationController = new TranslationController();
            var translationManager = new TranslationManager();
            var menu = new Menu(translationController, jobsController, logger, translationManager);
            
            menu.Run();
        
        }
    }
}
