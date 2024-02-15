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
    public enum OperationType { Display, Perform, Remove, Edit}
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
                        jobInfo.TotalFilesToCopy, jobInfo.NbFilesLeftToDo, jobInfo.TotalFilesSize);
                    
                    Jobs.Add(job);
                }
                
            }
            
        }

        private void HandleFileSaved(object sender, string fileName)
        {
            _logger.LogAction(fileName, "", "", 0, TimeSpan.Zero);
        }

        // Editing a job that exists -> used in another method EditJob(logger, translation)
        public void EditJob(int i, string name, string source, string destination, BackupType backupType, Logger logger)
        {
            Job job = Jobs[i];
            if (job != null)
            {
                
                job.SourceFilePath = source;
                job.TargetFilePath = destination;
                job.BackupType = backupType;
                
                // Using helpers to calculate the new directory's size info
                var _fileCopier = new FileCopier();
                DirectoryInfo diSource = new DirectoryInfo(job.SourceFilePath);
                long totalFilesSize = _fileCopier._fileGetter.DirSize(diSource);
                
                job.TotalFilesSize = totalFilesSize;
                job.NbFilesLeftToDo = totalFilesSize;
                job.NbSavedFiles = 0;

                logger.LogState(job.Name, job.SourceFilePath, job.TargetFilePath, job.State, job.TotalFilesToCopy, job.TotalFilesSize , (job.TotalFilesToCopy - job.NbSavedFiles), ((job.NbSavedFiles * 100) / job.TotalFilesToCopy), name);
                
                // Renaming for the current job object (LogState Constaints)
                job.Name = name;
            }
            else
            {
                Console.WriteLine(translation.JobsController.JobNotFound); 
            }
        }

        // Méthode pour supprimer un travail de sauvegarde
        public void DeleteJob(int i, TranslationModel translation, Logger logger)
        {
            Job job = Jobs[i];
            if (job != null)
            {
                job.State = JobState.Retired;
                logger.LogState(job.Name, job.SourceFilePath, job.TargetFilePath, JobState.Pending, job.TotalFilesToCopy, job.TotalFilesSize , (job.TotalFilesToCopy - job.NbSavedFiles), ((job.NbSavedFiles * 100) / job.TotalFilesToCopy), job.Name);
                Jobs.Remove(job);
                Console.Clear();
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

            if (Jobs.Count >= 5)
            {
                Console.WriteLine("Vous avez déjà atteint le nombre maximal de travaux de sauvegarde !");
                return;
            }
            
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

            // Convert user choice (Full/Diff)
            string type = backupType == "1" ? (translation.Messages.CompleteBackup) : backupType == "2" ? (translation.Messages.DifferentialBackup) : null;

            if (type != null)
            {
                BackupType typeSave = type == "1" ? BackupType.Full : BackupType.Diff;                
                var job = new Job(name, typeSave, source, destination);
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


        /* Used to displayJobs & perform operations on jobs */
        public void DisplayJobs(TranslationModel translation, Logger logger, OperationType op)
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

            string choice;

            
            // REMOVE JOB
            if (op == OperationType.Remove)
            {
                Console.WriteLine("\n");
                Console.Write(translation.Messages.Choice);

                choice = Console.ReadLine();
                int ch = int.Parse(choice);
                ch--;

                if (ch >= Jobs.Count || ch < 0 || Jobs.Count == 0)
                {
                    Console.WriteLine(translation.JobsController.JobNotFound);
                    return;
                }
                
                DeleteJob(ch, translation, logger);
                return;
            }
            
            // EDIT JOB
            if (op == OperationType.Edit)
            {
                return;
            }

            // DISPLAY JOB
            if (op == OperationType.Display)
            {
                return;
            }

            
            // PERFORM JOB

            Console.WriteLine("\n");
            Console.WriteLine(translation.Messages.LaunchJobSpecific);
            Console.WriteLine(translation.Messages.LaunchAllJobs);
            Console.WriteLine(translation.Messages.LaunchJobsRange);
            Console.WriteLine(translation.Messages.LaunchJobsInterval);


            Console.Write(translation.Messages.Choice);
            choice = Console.ReadLine();

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

                    if (ContainsDuplicates(choiceArrayStr))
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
                    Console.WriteLine(translation.Messages.NoJobSelected);
                    Console.Write(translation.Messages.Choice);
                    choice = Console.ReadLine();
                }
            }
            
            Console.WriteLine(translation.JobsController.ReturnToMenu);
            Console.ReadKey();
            Console.Clear();
        }

        public void EditJob( Logger logger, TranslationModel translation)
        {
            DisplayJobs(translation, logger, OperationType.Edit);

            Console.WriteLine(translation.Messages.Choice);
            string choice = Console.ReadLine();
            
            Console.Clear();

            if (int.TryParse(choice, out int index) && index > 0 && index <= Jobs.Count)
            {

                // Prompt user for new details
                Console.Write(translation.Messages.EnterBackupName);
                string newName = Console.ReadLine();

                Console.Write(translation.Messages.EnterSourceDirectory);
                string newSource = Console.ReadLine();

                Console.Write(translation.Messages.EnterTargetDirectory);
                string newDestination = Console.ReadLine();

                Console.WriteLine(translation.Messages.ChooseBackupType);
                Console.WriteLine($"1. {translation.Messages.CompleteBackup}");
                Console.WriteLine($"2. {translation.Messages.DifferentialBackup}");
                Console.Write(translation.Messages.Choice);
                string backupTypeChoice = Console.ReadLine();
                
                BackupType newBackupType = backupTypeChoice == "1" ? BackupType.Full : BackupType.Diff;
                
                EditJob(index - 1, newName, newSource, newDestination, newBackupType, logger);

                Console.WriteLine(translation.JobsController.JobEditedSuccessfully);
            }
            else
            {
                Console.WriteLine(translation.Messages.InvalidChoice);
            }

            Console.WriteLine(translation.JobsController.ReturnToMenu);
            Console.ReadKey();
            Console.Clear();
        }

        public void RemoveJob( Logger logger, TranslationModel translation)
        {
            DisplayJobs(translation, logger, OperationType.Remove);
            Console.WriteLine(translation.JobsController.ReturnToMenu);
            Console.ReadKey();
            Console.Clear();
        }
        static bool PatternRegEx(string text, Regex pattern)
        {
            Match m = pattern.Match(text);
            return m.Success;
        }

        bool ContainsDuplicates(Array array)
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
