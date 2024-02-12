using System;
using System.IO;

namespace EasySave.Models
{
    public class Logger
    {
        private readonly string logFilePath = @"C:\temp\log_journalier.txt";
        private static Logger instance;

        private Logger() { }

        public static Logger GetInstance()
        {
            if (instance == null)
            {
                instance = new Logger();
            }
            return instance;
        }

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

