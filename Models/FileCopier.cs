using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using EasySave.Library;
using EasySave.Views;

namespace EasySave.Models
{
    public class FileCopier
    {
        private Logger _logger = Logger.GetInstance();
        private readonly IdentityManager _identity = new IdentityManager();
        public FileGetter _fileGetter = new FileGetter();
        private ProgressBar _progressBar = new ProgressBar();

        public FileCopier() { }

        public void CopyDirectory(Job job, TranslationModel translation)
        {
            job.StartTime = DateTime.Now;
            job.State = JobState.Active;
            
            List<string> allFiles = _fileGetter.GetAllFiles(job.SourceFilePath);
            HashSet<string> loadedHashes = _identity.LoadAllowedHashes(job.Name);
            HashSet<string> allowedHashes = new HashSet<string>();

            job.TotalFilesToCopy = allFiles.Count();

            string message = translation.FileCopier.WarningMessage;
            bool warningAccepted = CopyWarning(message, translation);

            if (!warningAccepted)
                return;

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            // TODO : JobID /!\
            Console.Clear();
            Console.WriteLine(_progressBar.UpdateProgress(0, job.Name, job.TotalFilesToCopy, job.NbSavedFiles));
            
            if (job.BackupType == BackupType.Diff)
                CopyDiff(job, allFiles, allowedHashes, loadedHashes, translation);
            else
                CopyFull(job, allFiles, allowedHashes, translation);

            stopWatch.Stop();
            job.Duration = stopWatch.Elapsed;

            job.EndTime = DateTime.Now;
            job.State = JobState.Finished;
            
        }

        private void CopyDiff(Job job, List<string> allFiles, HashSet<string> allowedHashes, HashSet<string> loadedHashes, TranslationModel translation)
        {
            DirectoryInfo diSource = new DirectoryInfo(job.SourceFilePath);
            long totalFilesSize = _fileGetter.DirSize(diSource);

            foreach (string file in allFiles)
            {
                Stopwatch copyTime = new Stopwatch();
                copyTime.Start();
                
                string sourceHash = _identity.CalculateMD5(file);
                allowedHashes.Add(sourceHash);

                if (!loadedHashes.Contains(sourceHash))
                {
                    string relativePath = _fileGetter.GetRelativePath(job.SourceFilePath, file);
                    string targetFilePath = Path.Combine(job.TargetFilePath, relativePath);

                    string targetFileDir = Path.GetDirectoryName(targetFilePath);
                    if (!Directory.Exists(targetFileDir))
                        Directory.CreateDirectory(targetFileDir);
                    
                    File.Copy(file, targetFilePath, true);
                    
                    EndFileCopy(job, targetFilePath, file, copyTime, totalFilesSize);
                }
                else
                {
                    _logger.LogAction("Already exsits" + job.Name, file, "", 0, TimeSpan.Zero);
                }
            }

            job.State = JobState.Finished;
            _logger.LogState(job.Name, job.SourceFilePath, job.TargetFilePath, job.State, job.TotalFilesToCopy, totalFilesSize , (job.TotalFilesToCopy - job.NbSavedFiles), ((job.NbSavedFiles * 100) / job.TotalFilesToCopy), job.Name);
            
            _identity.DeleteAllowedHashes(job.Name);
            _identity.SaveAllowedHashes(allowedHashes, job.Name);
            _fileGetter.CompareAndDeleteDirectories(job.TargetFilePath, job.SourceFilePath);
        }

        private void CopyFull(Job job, List<string> allFiles, HashSet<string> allowedHashes, TranslationModel translation)
        {
            DirectoryInfo diSource = new DirectoryInfo(job.SourceFilePath);
            long totalFilesSize = _fileGetter.DirSize(diSource);
            
            _fileGetter.CleanTarget(job.TargetFilePath);

            foreach (string file in allFiles)
            {
                Stopwatch copyTime = new Stopwatch();
                copyTime.Start();
                string sourceHash = _identity.CalculateMD5(file);
                allowedHashes.Add(sourceHash);

                string relativePath = _fileGetter.GetRelativePath(job.SourceFilePath, file);
                string targetFilePath = Path.Combine(job.TargetFilePath, relativePath);

                string targetFileDir = Path.GetDirectoryName(targetFilePath);
                if (!Directory.Exists(targetFileDir))
                    Directory.CreateDirectory(targetFileDir);

                File.Copy(file, targetFilePath, true);

                EndFileCopy(job, targetFilePath, file, copyTime, totalFilesSize);
                
            }
            
            job.State = JobState.Finished;
            _logger.LogState(job.Name, job.SourceFilePath, job.TargetFilePath, job.State, job.TotalFilesToCopy, totalFilesSize , (job.TotalFilesToCopy - job.NbSavedFiles), ((job.NbSavedFiles * 100) / job.TotalFilesToCopy), job.Name);
            
            _identity.DeleteAllowedHashes(job.Name);
            _identity.SaveAllowedHashes(allowedHashes, job.Name);
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
        private void EndFileCopy(Job job, string targetFilePath, string file, Stopwatch copyTime, long totalFilesSize)
        {
            
            job.NbSavedFiles++;

            // TODO : JobID /!\
            Console.Clear();
            Console.WriteLine(_progressBar.UpdateProgress(0, job.Name, job.TotalFilesToCopy, job.NbSavedFiles));

            long fileSize = new System.IO.FileInfo(targetFilePath).Length;

            _logger.LogAction(job.Name, file, targetFilePath, fileSize, copyTime.Elapsed);

            job.State = JobState.Active;
            job.Progression = ((job.NbSavedFiles * 100) / job.TotalFilesToCopy);
            
            _logger.LogState(job.Name, job.SourceFilePath, job.TargetFilePath, job.State, job.TotalFilesToCopy, totalFilesSize , (job.TotalFilesToCopy - job.NbSavedFiles), ((job.NbSavedFiles * 100) / job.TotalFilesToCopy), job.Name);
                    
            copyTime.Stop();
            copyTime.Reset();
        }
    }
}
