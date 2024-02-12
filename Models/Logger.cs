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

        public void LogAction(string name, string fileSource, string fileTarget, long fileSize, double fileTransferTime)
        {
            string logMessage = $"{{\n" +
                                $" \"Name\": \"{name}\",\n" +
                                $" \"FileSource\": \"{fileSource}\",\n" +
                                $" \"FileTarget\": \"{fileTarget}\",\n" +
                                $" \"FileSize\": {fileSize},\n" +
                                $" \"FileTransferTime\": {fileTransferTime},\n" +
                                $" \"Time\": \"{DateTime.Now:dd/MM/yyyy HH:mm:ss}\"\n" +
                                $" }}";

            File.AppendAllText(logFilePath, logMessage + Environment.NewLine);
        }

        public void DisplayLog()
        {
            string logContents = File.ReadAllText(logFilePath);
            Console.WriteLine(logContents);
        }
    }
}

