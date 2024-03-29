﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using EasySave.Model;
using EasySave.Model.CopyHelper;
using EasySave.Model.Translation;
using EasySave.View;

namespace EasySave.Controller
{
    public class CopyController
    {
        private Logger _logger = Logger.GetInstance();
        private readonly IdentityManager _identity = new IdentityManager();
        public FileGetter _fileGetter = new FileGetter();
        private ProgressBar _progressBar = new ProgressBar();

        public CopyController() { }

        public void CopyDirectory(Job job, TranslationModel translation)
        {
            job.StartTime = DateTime.Now;
            job.State = JobState.Active;
            job.NbSavedFiles = 0;
            
            List<string> allFiles = _fileGetter.GetAllFiles(job.SourceFilePath);
            HashSet<string> loadedHashes = _identity.LoadAllowedHashes(job.Name);
            HashSet<string> allowedHashes = new HashSet<string>();

            job.TotalFilesToCopy = allFiles.Count();

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
