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

        public string Name
        {
            get => _name;
            set
            {
                if (value == _name) return;
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public BackupType BackupType
        {
            get => _backupType;
            set
            {
                if (value == _backupType) return;
                _backupType = value;
                OnPropertyChanged(nameof(BackupType));
            }
        }

        public string SourceFilePath
        {
            get => _sourceFilePath;
            set
            {
                if (value == _sourceFilePath) return;
                _sourceFilePath = value;
                OnPropertyChanged(nameof(SourceFilePath));
            }
        }

        public string TargetFilePath
        {
            get => _targetFilePath;
            set
            {
                if (value == _targetFilePath) return;
                _targetFilePath = value;
                OnPropertyChanged(nameof(TargetFilePath));
            }
        }

        private int Position { get; set; }
        private JobState _state { get; set; }

        public JobState State
        {
            get => _state;
            set
            {
                _state = value;
                OnPropertyChanged(nameof(State));
                OnPropertyChanged("State");
                OnPropertyChanged("CanRunJob");
            }
        }

        public long TotalFilesSize
        {
            get => _totalFilesSize;
            set
            {
                if (value == _totalFilesSize) return;
                _totalFilesSize = value;
                OnPropertyChanged(nameof(TotalFilesSize));
            }
        }

        private int _TotalFilesToCopy;
        public int TotalFilesToCopy
        {
            get => _TotalFilesToCopy;
            set
            {
                _TotalFilesToCopy = value;
                OnPropertyChanged(nameof(TotalFilesToCopy));
                OnPropertyChanged(nameof(Percentage));
                OnPropertyChanged("TotalFilesToCopy");
                OnPropertyChanged("Percentage");
            }
        }


        public bool CanRunJob
        {
            get => _state is JobState.Pending or JobState.Paused or JobState.Finished;
        }

        public int _NbSavedFiles;
        private string _name;
        private BackupType _backupType;
        private string _sourceFilePath;
        private string _targetFilePath;
        private long _totalFilesSize;
        private long _nbFilesLeftToDo;
        private TimeSpan _duration;
        private DateTime _startTime;
        private DateTime _endTime;
        private int _progression;

        public int NbSavedFiles
        {
            get => _NbSavedFiles;
            set
            {
                _NbSavedFiles = value;
                OnPropertyChanged(nameof(NbSavedFiles));
                OnPropertyChanged(nameof(Percentage));
                OnPropertyChanged("NbSavedFiles");
                OnPropertyChanged("Percentage");
            }
        }

        public long NbFilesLeftToDo
        {
            get => _nbFilesLeftToDo;
            set
            {
                if (value == _nbFilesLeftToDo) return;
                _nbFilesLeftToDo = value;
                OnPropertyChanged(nameof(NbFilesLeftToDo));
            }
        }

        public TimeSpan Duration
        {
            get => _duration;
            set
            {
                if (value.Equals(_duration)) return;
                _duration = value;
                OnPropertyChanged(nameof(Duration));
            }
        }

        public DateTime StartTime
        {
            get => _startTime;
            set
            {
                if (value.Equals(_startTime)) return;
                _startTime = value;
                OnPropertyChanged(nameof(StartTime));
            }
        }

        public DateTime EndTime
        {
            get => _endTime;
            set
            {
                if (value.Equals(_endTime)) return;
                _endTime = value;
                OnPropertyChanged(nameof(EndTime));
            }
        }

        public int Progression
        {
            get => _progression;
            set
            {
                if (value == _progression) return;
                _progression = value;
                OnPropertyChanged(nameof(Progression));
            }
        }

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
            _state = JobState.Pending;
            SourceFilePath = src;
            TargetFilePath = dest;
            
            TotalFilesToCopy = 0;
            NbSavedFiles = 0;
            
            _nextId++;
            _nextPos++;
        }
        
        /* Helper constructor for state.json */
        public Job(string name, BackupType type, JobState jobState, string src, string dest, int totalFilesToCopy, long nbFilesLeftToDo, long totalFilesSize) 
        {
            Id = _nextId;
            Position = _nextPos;
            Name = name;
            _state = jobState;
            SourceFilePath = src;
            TargetFilePath = dest;
            BackupType = type;
            
            TotalFilesToCopy = totalFilesToCopy;
            NbFilesLeftToDo = nbFilesLeftToDo;
            TotalFilesSize = totalFilesSize;

            _nextId++;
            _nextPos++;
        }
    }
}
