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
        private readonly string _path = Program.LogsDirectoryPath;

        public Logger()
        {

            if (!Directory.Exists(_path))
            {
                Directory.CreateDirectory(_path);
            }

            if (!File.Exists(Path.Combine(_path, @"\log_journalier.json")))
            {
                File.Create(Path.Combine(_path, @"\log_journalier.json"));
            }
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

