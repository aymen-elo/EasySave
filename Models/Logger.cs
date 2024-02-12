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

            if (!File.Exists(Path.Combine(_path, @"\log_journalier.txt")))
            {
                File.Create(Path.Combine(_path, @"\log_journalier.txt"));
            }
        }

        public void LogAction(string action)
        {
            string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {action}";
            File.AppendAllText(_path + @"\log_journalier.txt", logMessage + Environment.NewLine);
        }
        public void DisplayLog()
        {
            string logContents = File.ReadAllText(_path);
            Console.WriteLine(logContents);
        }
    }
}

