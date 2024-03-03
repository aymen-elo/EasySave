using System;
using EasySaveLib.Model;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySaveRemote.Packets
{
    [Serializable]
    public class JobLaunchPacket
    {
        public string JobName;
        public BackupType BackupType;
        public JobState BackupState;
        public string SourceDirectory;
        public string TargetDirectory;
    }
}