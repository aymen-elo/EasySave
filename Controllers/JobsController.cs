using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EasySave.Library;
using EasySave.Models;
using Newtonsoft.Json;

namespace EasySave.Controllers
{
    public class JobsController
    { 
        public List<Job> Jobs { get; private set; }
        private Logger _logger;
        TranslationModel translation;
        
        public event EventHandler<string> FileSaved;
        private FileCopier _fileCopier;

        public JobsController(Logger logger)
        {
            Jobs = new List<Job>();
            this._logger = logger;
            Initialize();
            
            
            /* Jobs List outside of app lifecycle */
            
            // TODO : Check state.json for job
            
            // abonnement  à l'événement FileSaved du BackupManager
            FileSaved += HandleFileSaved;
        }

        public List<Job> GetJobs()
        {
            return Jobs;
        }

        public void Initialize()
        {
            FileSaved += HandleFileSaved;
            
            /* Handling the already existing backup jobs (if they exist) */
            
            // Checking if the State logging file exists
            var stateFile = Program.LogsDirectoryPath + @"\state.json";
            if (File.Exists(stateFile) & new FileInfo(stateFile).Length != 0)
            {
                
                // Deserialize the Json to access its value through Job Class
                string jsonContent = File.ReadAllText(stateFile);
                var content = JsonConvert.DeserializeObject<List<Job>>(jsonContent);

                foreach (var jobInfo in content)
                {
                    // When the job is not taken account of -> we ignore it because it's not a Job anymore 
                    if (jobInfo.State == JobState.Retired) { continue; }
                    
                    // Construct a new Job with the available data in the json and append it to the Jobs List
                    var job = new Job(jobInfo.Name, jobInfo.State, jobInfo.SourceFilePath, jobInfo.TargetFilePath, 
                        jobInfo.TotalFilesToCopy, jobInfo.NbFilesLeftToDo);
                    
                    Jobs.Add(job);
                }
                
            }
            
        }

        private void HandleFileSaved(object sender, string fileName)
        {
            _logger.LogAction(fileName, "", "", 0, TimeSpan.Zero);
        }

        // Méthode pour créer un travail de sauvegarde
        public void CreateJob(string nom, string source, string destination, BackupType backupType)
        {
            Job newBackupJob = new Job(nom, backupType,source, destination);

            Jobs.Add(newBackupJob);
        }

        // Méthode pour modifier un travail de sauvegarde existant
        public void EditJob(string nom, string source, string destination, BackupType backupType)
        {
            Job existingJob = Jobs.FirstOrDefault(job => job.Name == nom);
            if (existingJob != null)
            {
                existingJob.SourceFilePath = source;
                existingJob.TargetFilePath = destination;
                existingJob.BackupType = backupType;
            }
            else
            {
                Console.WriteLine(translation.JobsController.JobNotFound); 
            }
        }

        // Méthode pour supprimer un travail de sauvegarde
        public void DeleteJob(string name)
        {
            Job job = Jobs.FirstOrDefault(job => job.Name == name);
            if (job != null)
            {
                job.State = JobState.Retired;
                Jobs.Remove(job);
                Console.WriteLine(translation.JobsController.JobDeletedSuccessfully);
            }
            else
            {
                Console.WriteLine(translation.JobsController.JobNotFound);
            }
        }
        
        public void AddJob(Logger logger, TranslationModel translation, Menu menu)
        {
            Console.Clear();
            
            
            /* Backup Name */
            Regex rg = new Regex(@"^[a-zA-Z0-9\s]*$");
            Console.Write(translation.Messages.EnterBackupName);

            string name = Console.ReadLine();
            while (!PatternRegEx(name, rg))
            {
                Console.WriteLine(translation.Messages.InvalidBackupName);
                Console.Write(translation.Messages.EnterBackupName);
                name = Console.ReadLine();
            }
            
            /* Backup Source */ 
            rg = new Regex(@"^[a-zA-Z]:\\(?:[^<>:""/\\|?*]+\\)*[^<>:""/\\|?*]*$");
            Console.Write(translation.Messages.SourceDirectory);

            string source = Console.ReadLine();
            while (!PatternRegEx(source, rg))
            {
                Console.WriteLine(translation.Messages.InvalidBackupDirectory);
                Console.Write(translation.Messages.SourceDirectory);
                source = Console.ReadLine();
            }
            
            /* Backup Destination */
            Console.Write(translation.Messages.DestinationDirectory);
            string destination = Console.ReadLine();
            while (!PatternRegEx(destination, rg))
            {
                Console.WriteLine(translation.Messages.InvalidBackupDirectory);
                Console.Write(translation.Messages.DestinationDirectory);
                destination = Console.ReadLine();
            }

            // Backup Type Choice
            Console.WriteLine(translation.Messages.ChooseBackupType);
            Console.WriteLine($"1. {translation.Messages.CompleteBackup}");
            Console.WriteLine($"2. {translation.Messages.DifferentialBackup}");
            Console.Write(translation.Messages.Choice);
            string backupType = Console.ReadLine();

            // Convertir le choix de l'utilisateur en type de sauvegarde
            string type = backupType == "1" ? (translation.Messages.CompleteBackup) : backupType == "2" ? (translation.Messages.DifferentialBackup) : null;

            if (type != null)
            {
                // Créer un objet BackupJob avec les informations saisies
                var job = new Job(name, BackupType.Full, source, destination);

                // Ajouter le travail de sauvegarde en appelant la méthode correspondante du contrôleur
                Jobs.Add(job);
            }
            else
            {
                Console.WriteLine(translation.Messages.InvalidTypeChoice);
            }
        }

        public void LaunchJob(Job job, Logger logger, TranslationModel translation)
        {
            if (job.State == JobState.Finished || job.State == JobState.Pending)
            {
                job.Progression = 0;
                job.State = JobState.Active;
                job.NbFilesLeftToDo = job.TotalFilesToCopy;
            }
            
            logger.LogAction(job.Name, job.SourceFilePath, job.TargetFilePath, 0, TimeSpan.Zero);
            
            var fileCopier = new FileCopier();
            fileCopier.CopyDirectory(job, translation);
            
            if (job.Progression >= 100)
            {
                job.State = JobState.Finished;
            }

        }

        public void LaunchAllJobs(Logger logger, TranslationModel translation)
        {
            foreach (var job in Jobs)
            {
                LaunchJob(job, logger, translation);
            }
        }

        public void DisplayJob(Job job, TranslationModel translation, Logger logger)
        {
            Console.WriteLine(translation.Messages.ListBackupJobs);

            if (Jobs.Count == 0)
            {
                Console.WriteLine(translation.Messages.EmptyJobsList);
                return;
            }

            for (int j = 0; j < Jobs.Count; j++)
            {
                if (Jobs[j].Name == job.Name)
                {
                    Console.WriteLine($"-> {translation.Messages.EnterBackupName} : {job.Name}, {translation.Messages.SourceDirectory} : {job.SourceFilePath}, {translation.Messages.DestinationDirectory} : {job.TargetFilePath}, {translation.Messages.ChooseBackupType} {job.BackupType}");
                    break;
                }
            }
            
            Console.WriteLine("Lancer ? y/n");
            var choice = Console.ReadLine();

            if (choice == "y")
            {
                LaunchJob(job, logger, translation);
            }
            
            Console.WriteLine(translation.JobsController.ReturnToMenu);
            Console.ReadKey();
            Console.Clear();
        }

        public void DisplayJobs(TranslationModel translation, Logger logger)
        {
            Console.WriteLine(translation.Messages.ListBackupJobs);

            if (Jobs.Count == 0)
            {
                Console.WriteLine(translation.Messages.EmptyJobsList);
                return;
            }
            
            Console.WriteLine(" (0) - " + translation.Menu.BackupManage);
            int i = 0;
            foreach (var travail in this.GetJobs())
            {
                Console.WriteLine(
                    $" ({i + 1}) - {translation.Messages.EnterBackupName} {travail.Name}, " +
                    $"{translation.Messages.SourceDirectory} {travail.SourceFilePath}, " +
                    $"{translation.Messages.DestinationDirectory} {travail.TargetFilePath}, " +
                    $"{translation.Messages.ChooseBackupType} {travail.BackupType}");
                i++;
            }


            Console.WriteLine("\n");
            Console.WriteLine(translation.Messages.LaunchJobSpecific);
            Console.WriteLine(translation.Messages.LaunchAllJobs);
            Console.WriteLine(translation.Messages.LaunchJobsRange);
            Console.WriteLine(translation.Messages.LaunchJobsInterval);


            Console.Write(translation.Messages.Choice);
            var choice = Console.ReadLine();

            if (choice == "0")
            {
                Console.Clear();
                return;
            }

            var isValid = false;
            while (!isValid) {
                // Interval of jobs : x to z => Backup(x,y,z)
                if (choice.Contains("-") & Regex.IsMatch(choice, @"^[1-9]\d*-[1-9]\d*$"))
                {
                    isValid = true;
                    
                    var choiceArray = choice.Split("-");
                    int begin = int.Parse(choiceArray[0]);
                    int end = int.Parse(choiceArray[1]);

                    for (int k = begin; k <= end; k++)
                    {
                        LaunchJob(Jobs[k - 1], logger, translation);
                    }
                }
                // Specific set of Jobs : x, z => Backup(x,z)
                else if (choice.Contains(",") & Regex.IsMatch(choice, @"^[1-9]\d*(,[1-9]\d*)*$"))
                {
                    isValid = true;


                    string[] choiceArrayStr = choice.Split(",");
                    int[] choiceArray = Array.ConvertAll(choiceArrayStr, int.Parse);

                    if (containsDuplicates(choiceArrayStr))
                    {
                        isValid = false;
                    }
                    else
                    {
                        int it = 1;
                        foreach (var job in Jobs)
                        {
                            if (choiceArray.Contains(it))
                            {
                                LaunchJob(job, logger, translation);
                            }
    
                            it++;
                        }
                    }
                }
                else if (choice == "A" & Regex.IsMatch(choice, @"^A$"))
                {
                    isValid = true;
                    
                    LaunchAllJobs(logger, translation);
                }
                // One and only one job to perform
                else if (Regex.IsMatch(choice, @"^[1-9]\d*$"))
                {
                    isValid = true;
                    
                    LaunchJob(Jobs[int.Parse(choice) - 1], logger, translation);
                }
                else
                {
                    Console.WriteLine(translation.Messages.InvalidResponse);
                    Console.Write(translation.Messages.Choice);
                    choice = Console.ReadLine();
                }
            }
            
            Console.WriteLine(translation.JobsController.ReturnToMenu);
            Console.ReadKey();
            Console.Clear();
        }

        public static void EditJob(JobsController jobsController)
        {
            // Implémenter la logique de modification d'un travail de sauvegarde
            // Utiliser les méthodes du contrôleur pour modifier un travail existant
        }

        public void RemoveJob(JobsController jobsController, Logger logger)
        {
            Console.Write(translation.JobsController.BackupNameToDelete); 
            string jobName = Console.ReadLine();
            
            jobsController.DeleteJob(jobName);
            logger.LogAction(jobName, "", "", 0, TimeSpan.Zero);
        }
        static bool PatternRegEx(string text, Regex pattern)
        {
            Match m = pattern.Match(text);
            return m.Success;
        }

        bool containsDuplicates(Array array)
        {
            HashSet<object> set = new HashSet<object>();
            foreach (var item in array)
            {
                if (!set.Add(item))
                {
                    return true; 
                }
            }
            return false;
        }
    }
}
