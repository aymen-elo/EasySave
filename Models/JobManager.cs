using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave.Models
{
    public class JobManager
    {
        public event EventHandler<string> FileSaved;
        public List<Job> Jobs { get; private set; }
        
        public JobManager()
        {
            Jobs = new List<Job>();
        }
        
        // Save one file by name
        public void SaveFile(string fileName)
        {
            // Logique de sauvegarde (simulation)
            Console.WriteLine($"Le fichier {fileName} a été sauvegardé.");

            // Déclenchement de l'événement de sauvegarde
            FileSaved?.Invoke(this, fileName);
        }

        // TODO : Exceptions
        internal void AddJob(Job job)
        {
            Jobs.Add(job);
        }
        internal void RemoveJob(Job job)
        {
            Jobs.Remove(job);
        }
    }
}
