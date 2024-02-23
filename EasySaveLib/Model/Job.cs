using System;
using System.ComponentModel;

namespace EasySaveLib.Model
{

    /* Pending = Hasn't started yet */
    /* Paused  = Started but was paused */
    /* Active  = Currently executing backup*/
    public enum JobState { Pending, Active, Paused , Finished, Retired}
    public enum BackupType{ Full, Diff }

    public class Job : INotifyPropertyChanged
    {
        // Managing assignment of ids and positions
        private static int _nextId = 0;
        private static int _nextPos = 0;
        
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
        
        private int Id { get; set; }
        public string Name { get; set; }
        public BackupType BackupType { get; set; }
        public string SourceFilePath { get; set; }
        public string TargetFilePath { get; set; }
        private int Position { get; set; }
        public JobState State { get; set; }
    
        public long TotalFilesSize { get; set; }
        private int _TotalFilesToCopy;
        public int TotalFilesToCopy
        {
            get => _TotalFilesToCopy;
            set
            {
                _TotalFilesToCopy = value;
                OnPropertyChanged("TotalFilesToCopy");
                OnPropertyChanged("Percentage");
            }
        }

        public int _NbSavedFiles;

        public int NbSavedFiles
        {
            get => _NbSavedFiles;
            set
            {
                _NbSavedFiles = value;
                OnPropertyChanged("NbSavedFiles");
                OnPropertyChanged("Percentage");
            }
        }

        public long NbFilesLeftToDo { get; set; }
        public TimeSpan Duration { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int Progression { get; set; }

        public double Percentage
        {
            get
            {
                return NbSavedFiles / (double)TotalFilesToCopy * 100;
            }
        }

        public Job() { }

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
        
        /* Helper constructor for state.json */
        public Job(string name, JobState jobState, string src, string dest, int totalFilesToCopy, long nbFilesLeftToDo, long totalFilesSize) 
        {
            Id = _nextId;
            Position = _nextPos;
            Name = name;
            State = jobState;
            SourceFilePath = src;
            TargetFilePath = dest;
            
            TotalFilesToCopy = totalFilesToCopy;
            NbFilesLeftToDo = nbFilesLeftToDo;
            TotalFilesSize = totalFilesSize;

            _nextId++;
            _nextPos++;
        }
    }
}
