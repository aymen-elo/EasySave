using System;
using System.IO;

namespace EasySave.Models
{
    public class Logger
    {
        private readonly string _path = Program.LogsDirectoryPath;
        private static Logger instance;

        private Logger()
        {

            if (!Directory.Exists(_path))
            {
                Directory.CreateDirectory(_path);
            }

            if (!File.Exists(_path + @"\log_journalier.json"))
            {
                Console.WriteLine("here");
                File.Create(_path + @"\log_journalier.json");
            }
        }
        
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
            File.AppendAllText(_path + @"\log_journalier.json", logMessage + Environment.NewLine);
        }

        public void DisplayLog()
        {
            string logContents = File.ReadAllText(_path);
            Console.WriteLine(logContents);
        }
    }
}

