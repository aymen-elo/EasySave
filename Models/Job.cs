using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace EasySave.Models
{

    /* Pending = Hasn't started yet */
    /* Paused  = Started but was paused */
    /* Active  = Currently executing backup*/
    public enum JobState { Pending, Active, Paused , Finished}
    public enum BackupType{ Full, Diff }

    public class Job
    {
        // Managing assignment of ids and positions
        private static int _nextId = 0;
        private static int _nextPos = 0;
        
        private int Id { get; set; }
        public string BackupName { get; private set; }
        public BackupType BackupType { get; set; }
        public string Source { get; set; }
        public string Destination { get; set; }
        private int Position { get; set; }
        public JobState State { get; set; }
    
        public int NbTotalFiles { get; set; }
        public int NbSavedFiles { get; set; }
        public TimeSpan Duration { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        

        public Job(string backupName, BackupType type, string src, string dest) 
        {
            Id = _nextId;
            Position = _nextPos;
            BackupName = backupName;
            BackupType = type;
            State = JobState.Pending;
            Source = src;
            Destination = dest;
            
            NbTotalFiles = -1;
            NbSavedFiles = -1;
            
            _nextId++;
            _nextPos++;
        }

        public bool Begin(string name, string source, string target, int type) 
        {
            return true; // si pas erreur lors lancement, renvoyé 1
        }
    }
}
