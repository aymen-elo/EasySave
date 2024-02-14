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
    public enum JobState { Pending, Active, Paused , Finished, Retired}
    public enum BackupType{ Full, Diff }

    public class Job
    {
        // Managing assignment of ids and positions
        private static int _nextId = 0;
        private static int _nextPos = 0;
        
        private int Id { get; set; }
        public string Name { get; set; }
        public BackupType BackupType { get; set; }
        public string SourceFilePath { get; set; }
        public string TargetFilePath { get; set; }
        private int Position { get; set; }
        public JobState State { get; set; }
    
        public int TotalFilesToCopy { get; set; }
        public int NbSavedFiles { get; set; }
        public int NbFilesLeftToDo { get; set; }
        public TimeSpan Duration { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int Progession { get; set; }
        

        public Job(string name, BackupType type, string src, string dest) 
        {
            Id = _nextId;
            Position = _nextPos;
            Name = name;
            BackupType = type;
            State = JobState.Pending;
            SourceFilePath = src;
            TargetFilePath = dest;
            
            TotalFilesToCopy = 0;
            NbSavedFiles = 0;
            
            _nextId++;
            _nextPos++;
        }
        
        public Job() {}
        
        /* Helper constructor for state.json */
        public Job(string name, JobState jobState, string src, string dest, int totalFilesToCopy, int nbFilesLeftToDo) 
        {
            Id = _nextId;
            Position = _nextPos;
            Name = name;
            State = jobState;
            SourceFilePath = src;
            TargetFilePath = dest;
            
            TotalFilesToCopy = totalFilesToCopy;
            NbFilesLeftToDo = nbFilesLeftToDo;

            _nextId++;
            _nextPos++;
        }

        public bool Begin(string name, string source, string target, int type) 
        {
            return true; // si pas erreur lors lancement, renvoyé 1
        }
    }
}
