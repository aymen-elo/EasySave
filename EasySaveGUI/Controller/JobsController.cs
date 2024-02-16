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
using Newtonsoft.Json.Linq;
using EasySave_2._0;
using System.ComponentModel;


namespace EasySave.Controllers
{
    public enum OperationType { Display, Perform, Remove, Edit}
    public class JobsController
    { 
        public List<Job> Jobs { get; private set; }
        private Logger _logger;
        TranslationModel translation;
        
        public event EventHandler<string> FileSaved;
        private CopyController _copyController;

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
                var _fileCopier = new CopyController();
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
                logger.LogState(job.Name, job.SourceFilePath, job.TargetFilePath, job.State, job.TotalFilesToCopy, job.TotalFilesSize , (job.TotalFilesToCopy - job.NbSavedFiles), ((job.NbSavedFiles * 100) / job.TotalFilesToCopy), job.Name);
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
                Console.WriteLine(translation.JobsController.BackupFull);
                return;
            }
            
            /* Backup Name */
            Regex rg = new Regex(@"^(?=.*[a-zA-Z0-9])[a-zA-Z0-9\s]*$");
            Console.Write(translation.Messages.EnterBackupName);

            string name = Console.ReadLine();
            while (!PatternRegEx(name, rg) || name == string.Empty || JobExists(name))
            {
                Console.WriteLine(translation.Messages.InvalidBackupName);
                Console.Write(translation.Messages.EnterBackupName);
                name = Console.ReadLine();
                Console.Clear();
            }
            
            /* Backup Source */ 
            rg = new Regex(@"^[a-zA-Z]:\\(?:[^<>:""/\\|?*]+\\)*[^<>:""/\\|?*]*$");
            Console.Write(translation.Messages.SourceDirectory);

            string source = Console.ReadLine();

            bool pathExist = Directory.Exists(source);
            
            while (!PatternRegEx(source, rg) || !pathExist)
            {
                Console.WriteLine(translation.Messages.InvalidBackupDirectory);
                Console.Write(translation.Messages.SourceDirectory);
                source = Console.ReadLine();
                pathExist = Directory.Exists(source);
                Console.Clear();
            }
            
            /* Backup Destination */
            Console.Write(translation.Messages.DestinationDirectory);
            string destination = Console.ReadLine();
            pathExist = Directory.Exists(source);

            while (!PatternRegEx(destination, rg) || !pathExist)
            {
                Console.WriteLine(translation.Messages.BackupPathNotFound);
                Console.Write(translation.Messages.DestinationDirectory);
                destination = Console.ReadLine();
                pathExist = Directory.Exists(destination);
                Console.Clear();
            }

            // Backup Type Choice
            Console.WriteLine(translation.Messages.ChooseBackupType);
            Console.WriteLine($"1. {translation.Messages.CompleteBackup}");
            Console.WriteLine($"2. {translation.Messages.DifferentialBackup}");
            Console.Write(translation.Messages.Choice);
            string backupType = Console.ReadLine();
            Console.Clear();

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

        public void LaunchJob(Job job, Logger logger, TranslationModel translation, BackgroundWorker worker)
        {
            if (job.State == JobState.Finished || job.State == JobState.Pending)
            {
                job.Progression = 0;
                job.State = JobState.Active;
                job.NbFilesLeftToDo = job.TotalFilesToCopy;
            }
            
            logger.LogAction(job.Name, job.SourceFilePath, job.TargetFilePath, 0, TimeSpan.Zero);
            
            var fileCopier = new CopyController();
            

            fileCopier.CopyDirectory(job, translation, worker);
            
            if (job.Progression >= 100)
            {
                job.State = JobState.Finished;
            }

        }

        public void LaunchAllJobs(Logger logger, TranslationModel translation, BackgroundWorker worker)
        {
            foreach (var job in Jobs)
            {
                LaunchJob(job, logger, translation, worker);
            }
        }


        /* Used to displayJobs & perform operations on jobs */
        public void DisplayJobs(TranslationModel translation, Logger logger, OperationType op, BackgroundWorker worker)
        {
            Console.Clear();
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
                
                Regex rg = new Regex(@"^(?=.*[0-9])[0-9\s]*$");

                while (!PatternRegEx(choice, rg) || choice == string.Empty)
                {
                    Console.WriteLine(translation.Messages.InvalidBackupNumber);
                    Console.Write(translation.Messages.EnterBackupName);
                    choice = Console.ReadLine();
                }
                
                int ch = int.Parse(choice);
                ch--;

                if (ch >= Jobs.Count || ch < 0 || Jobs.Count == 0)
                {
                    Console.WriteLine(translation.JobsController.JobNotFound);
                    return;
                }

                if (CopyWarning(translation.Messages.JobDeleting, translation))
                {
                    DeleteJob(ch, translation, logger);
                    return;
                }
                else
                {
                    Console.WriteLine(translation.Messages.NojobDeleted);
                    return;
                }
                
            }
            
            // EDIT JOB
            if (op == OperationType.Edit)
            {
                return;
            }

            // DISPLAY JOB
            if (op == OperationType.Display)
            {
                Console.WriteLine(translation.JobsController.ReturnToMenu);
                Console.ReadKey();
                Console.Clear();
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

                    string message = translation.FileCopier.WarningMessage;
                    bool warningAccepted = CopyWarning(message, translation);

                    if (!warningAccepted) 
                        return;
                    
                    for (int k = begin; k <= end; k++)
                    {
                        LaunchJob(Jobs[k - 1], logger, translation, worker);
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
                        string message = translation.FileCopier.WarningMessage;
                        bool warningAccepted = CopyWarning(message, translation);

                        if (!warningAccepted) 
                            return;
                        int it = 1;
                        foreach (var job in Jobs)
                        {
                            if (choiceArray.Contains(it))
                            {
                                LaunchJob(job, logger, translation, worker);
                            }
    
                            it++;
                        }
                    }
                }
                else if (choice == "A" & Regex.IsMatch(choice, @"^A$"))
                {
                    string message = translation.FileCopier.WarningMessage;
                    bool warningAccepted = CopyWarning(message, translation);

                    if (!warningAccepted) 
                        return;
                    
                    isValid = true;
                    
                    LaunchAllJobs(logger, translation, worker);
                }
                // One and only one job to perform
                else if (Regex.IsMatch(choice, @"^[1-9]\d*$"))
                {
                    string message = translation.FileCopier.WarningMessage;
                    bool warningAccepted = CopyWarning(message, translation);

                    if (!warningAccepted) 
                        return;
                    
                    isValid = true;
                    
                    LaunchJob(Jobs[int.Parse(choice) - 1], logger, translation, worker);
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

        public void EditJob( Logger logger, TranslationModel translation, BackgroundWorker worker)
        {
            DisplayJobs(translation, logger, OperationType.Edit, worker);

            Console.WriteLine(translation.Messages.Choice);
            string choice = Console.ReadLine();
            
            Console.Clear();

            if (int.TryParse(choice, out int index) && index > 0 && index <= Jobs.Count)
            {

                
                // Prompt user for new details
                Console.Write(translation.Messages.EnterBackupName);
                string newName = Console.ReadLine();
                Regex rg = new Regex(@"^(?=.*[a-zA-Z0-9])[a-zA-Z0-9\s]*$");
                
                while (!PatternRegEx(newName, rg) || newName == string.Empty || JobExists(newName))
                {
                    Console.WriteLine(translation.Messages.InvalidBackupDirectory);
                    Console.Write(translation.Messages.DestinationDirectory);
                    newName = Console.ReadLine();
                }
                
                Console.Write(translation.Messages.EnterSourceDirectory);
                string newSource = Console.ReadLine();
                bool pathExist = Directory.Exists(newSource);
                
                rg = new Regex(@"^[a-zA-Z]:\\(?:[^<>:""/\\|?*]+\\)*[^<>:""/\\|?*]*$");
                    
                while (!PatternRegEx(newSource, rg) || !pathExist)
                {
                    Console.WriteLine(translation.Messages.InvalidBackupDirectory);
                    Console.Write(translation.Messages.DestinationDirectory);
                    newSource = Console.ReadLine();
                    pathExist = Directory.Exists(newSource);
                }

                Console.Write(translation.Messages.EnterTargetDirectory);
                string newDestination = Console.ReadLine();
                pathExist = Directory.Exists(newDestination);
                
                while (!PatternRegEx(newSource, rg) || !pathExist)
                {
                    Console.WriteLine(translation.Messages.InvalidBackupDirectory);
                    Console.Write(translation.Messages.DestinationDirectory);
                    newDestination = Console.ReadLine();
                    pathExist = Directory.Exists(newDestination);

                }

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

        public void RemoveJob( Logger logger, TranslationModel translation, BackgroundWorker worker)
        {
            DisplayJobs(translation, logger, OperationType.Remove, worker);
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
        private bool CopyWarning(string message, TranslationModel translation)
        {
            Console.WriteLine(message);

            Console.Write(translation.FileCopier.Continue);
            string response = Console.ReadLine();

            if (response.ToLower() == "y")
                return true;
            else if (response.ToLower() == "n")
                return false;
            else
            {
                Console.WriteLine(translation.FileCopier.InvalidResponseFileCopier);
                return CopyWarning(message, translation);
            }
        }

        private bool JobExists(string name)
        {
            foreach (var job in Jobs)
            {
                if (job.Name == name)
                {
                    return true;
                }
            }

            return false;
        }
        
    }
}
