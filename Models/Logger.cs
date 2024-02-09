using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave.Models
{
    public class Logger
    {
        private readonly string logFilePath = @"C:\temp\log_journalier.txt";

        public void LogAction(string action)
        {
            string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {action}";

            File.AppendAllText(logFilePath, logMessage + Environment.NewLine);
        }
    }
}

