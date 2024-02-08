using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace EasySave.Library
{

    /* Pending = Hasn't started yet */
    /* Paused  = Started but was paused */
    /* Active  = Currently executing backup*/
    enum JobState
    {
        Pending,
        Active,
        Paused
    }

    class Job
    {
        private static int NextId = 0;
        private static int NextPos = 0;
        private int Id { get; set; } // voir fonction incrémentation auto
        private JobState State { get; set; }
        private int Position { get; set; }
        private int NbTotalFiles { get; set; }
        private int NbSavedFiles { get; set; }

        private Backup Backup { get; set; }
        private TimeSpan duration { get; set; }
        public Job(Backup b) 
        {
            this.Id = NextId;
            this.State = JobState.Pending;
            this.Position = NextPos;

            this.Backup = b;

            

            NextId++;
            NextPos++;
        }
        public bool Begin(string name, string source, string target, int type) 
        {
            return true; // si pas erreur lors lancement, renvoyé 1
        }
    }
}
