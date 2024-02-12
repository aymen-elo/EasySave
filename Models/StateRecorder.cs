using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave.Models
{
    public class StateRecorder
    {
        private readonly string _path = Program.LogsDirectoryPath;

        public void RecordState(string state)
        {
            string stateMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {state}";

            try
            {
                File.WriteAllText(_path + @"\etat_avancement.txt", stateMessage);
            }
            catch (Exception e)
            {
                Directory.CreateDirectory(_path);
                File.Create(Path.Combine(_path, @"\etat_avancement.txt"));
                File.WriteAllText(Path.Combine(_path,@"\etat_avancement.txt"), stateMessage);
            }
        }
    }
}
