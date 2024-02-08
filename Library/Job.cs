using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave.Library
{
     class Job
    {
        private int id { get; set; } // voir fonction incrémentation auto
        private string name { get; set; }
        private string source { get; set; }
        private string target { get; set; }
        private int type { get; set; }
        private int state { get; set; }
        private int position { get; set; }
        public Job() 
        {
            id = 0; // a modif 
            name = string.Empty;
            source = string.Empty;
            target = string.Empty;
            type = 0;
            state = 0;
            position = 0;
        }
        public bool begin(string name, string source, string target, int type) 
        {
            return true; // si pas erreur lors lancement, renvoyé 1
        }
        public int getState(int id) 
        {
            return state;
        }
        public int getPosition(int id) 
        {
            return position;
        }
    }
}
