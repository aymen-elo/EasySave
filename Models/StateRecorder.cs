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
        private readonly string stateFilePath = @"C:\temp\etat_avancement.txt";

        public void RecordState(string state)
        {
            string stateMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {state}";
            File.WriteAllText(stateFilePath, stateMessage);
        }
    }
}
