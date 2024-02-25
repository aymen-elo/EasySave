using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using EasySaveLib.Model;
using EasySaveLib.Model.CopyHelper;
using System.Threading;
using EasySaveGUI.Model;
using EasySaveGUI.View;
using EasySaveGUI.ViewModel;

namespace EasySaveGUI.Controller
{
    public class CopyController
    {
        private readonly Logger _logger;
        private readonly IdentityManager _identity;
        public FileGetter _fileGetter;
        private readonly ProgressBar _progressBar;
        
        /* Multi threading
         DONE : Full Backup
         TODO : Diff Backup */
        /* Separation of a file list into
         ThreadCount number of chunks */
        private List<List<string>> _fileChunks;
        private const int ThreadCount = 5;

        public CopyController()
        {
            _logger = Logger.GetInstance();
            _identity = new IdentityManager();
            _fileGetter = new FileGetter();
            _progressBar = new ProgressBar();
            _fileChunks = new List<List<string>>();
        }

        public void CopyDirectory(Job job)
        {
            job.StartTime = DateTime.Now;
            job.State = JobState.Active;
            job.NbSavedFiles = 0;
            
            List<string> allFiles = _fileGetter.GetAllFiles(job.SourceFilePath);
            HashSet<string> loadedHashes = _identity.LoadAllowedHashes(job.Name);
            var allowedHashes = new ConcurrentDictionary<string, byte>();

            job.TotalFilesToCopy = allFiles.Count();

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            // TODO : JobID /!\
            
            Console.WriteLine(_progressBar.UpdateProgress(0, job.Name, job.TotalFilesToCopy, job.NbSavedFiles));

            // Splitting file by Number of Threads
            _fileChunks = ChunkFiles(allFiles, ThreadCount);
            
            foreach (var chunk in _fileChunks)
            {
                ThreadPool.QueueUserWorkItem(state =>
                {
                    if (job.BackupType == BackupType.Diff)
                        CopyDiff(job, chunk, allowedHashes, loadedHashes);
                    else
                        CopyFull(job, chunk, allowedHashes);
                });
            }
            WaitForThreadPoolCompletion();

            stopWatch.Stop();
            job.Duration = stopWatch.Elapsed;

            job.EndTime = DateTime.Now;
            job.State = JobState.Finished;
        }

        private void CopyDiff(Job job, List<string> allFiles, ConcurrentDictionary<string, byte> allowedHashes, HashSet<string> loadedHashes)
        {
            DirectoryInfo diSource = new DirectoryInfo(job.SourceFilePath);
            long totalFilesSize = _fileGetter.DirSize(diSource);

            foreach (string file in allFiles)
            {
                Stopwatch copyTime = new Stopwatch();
                copyTime.Start();
                
                string sourceHash = _identity.CalculateMD5(file);
                allowedHashes.TryAdd(sourceHash, 0);

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
            _identity.SaveAllowedHashes(allowedHashes.Keys, job.Name);
            _fileGetter.CompareAndDeleteDirectories(job.TargetFilePath, job.SourceFilePath);
        }

        private void CopyFull(Job job, List<string> chunk, ConcurrentDictionary<string, byte> allowedHashes)
        {
            DirectoryInfo diSource = new DirectoryInfo(job.SourceFilePath);
            long totalFilesSize = _fileGetter.DirSize(diSource);
            
            // TODO : Warn about wiping off the Target
            _fileGetter.CleanTarget(job.TargetFilePath);

            foreach (string file in chunk)
            {
                Stopwatch copyTime = new Stopwatch();
                copyTime.Start();
                string sourceHash = _identity.CalculateMD5(file);
                allowedHashes.TryAdd(sourceHash, 0);

                string relativePath = _fileGetter.GetRelativePath(job.SourceFilePath, file);
                string targetFilePath = Path.Combine(job.TargetFilePath, relativePath);

                string targetFileDir = Path.GetDirectoryName(targetFilePath);
                if (!Directory.Exists(targetFileDir))
                    Directory.CreateDirectory(targetFileDir);

                File.Copy(file, targetFilePath, true);

                EndFileCopy(job, targetFilePath, file, copyTime, totalFilesSize);
            }
            chunk.Clear();
            
            job.State = JobState.Finished;
            _logger.LogState(job.Name, job.SourceFilePath, job.TargetFilePath, job.State, job.TotalFilesToCopy, totalFilesSize , (job.TotalFilesToCopy - job.NbSavedFiles), ((job.NbSavedFiles * 100) / job.TotalFilesToCopy), job.Name);
            
            _identity.DeleteAllowedHashes(job.Name);
            _identity.SaveAllowedHashes(allowedHashes.Keys, job.Name);
        }
        
        private void EndFileCopy(Job job, string targetFilePath, string file, Stopwatch copyTime, long totalFilesSize)
        {
            
            job.NbSavedFiles++;

            // TODO : JobID /!\
            
            Console.WriteLine(_progressBar.UpdateProgress(0, job.Name, job.TotalFilesToCopy, job.NbSavedFiles));

            long fileSize = new System.IO.FileInfo(targetFilePath).Length;

            _logger.LogAction(job.Name, file, targetFilePath, fileSize, copyTime.Elapsed);

            job.State = JobState.Active;
            job.Progression = ((job.NbSavedFiles * 100) / job.TotalFilesToCopy);
            
            _logger.LogState(job.Name, job.SourceFilePath, job.TargetFilePath, job.State, job.TotalFilesToCopy, totalFilesSize , (job.TotalFilesToCopy - job.NbSavedFiles), ((job.NbSavedFiles * 100) / job.TotalFilesToCopy), job.Name);
                    
            copyTime.Stop();
            copyTime.Reset();
            
            List<string> processBlackList = new List<string>();
            
            processBlackList.Add("calc");
            processBlackList.Add("CalculatorApp");
            
            ProcessBL.IsProcessRunning(processBlackList);
            
            while (MainWindow.IsPaused || ProcessBL.IsDetected)
            {  
                Thread.Sleep(100);
                ProcessBL.IsProcessRunning(processBlackList);
            }
        }
        
        private List<List<string>> ChunkFiles(List<string> files, int chunkCount)
        {
            List<List<string>> chunks = new List<List<string>>();
            int chunkSize = (int)Math.Ceiling((double)files.Count / chunkCount);

            for (int i = 0; i < files.Count; i += chunkSize)
            {
                chunks.Add(files.GetRange(i, Math.Min(chunkSize, files.Count - i)));
            }

            return chunks;
        }
        
        private void WaitForThreadPoolCompletion()
        {
            while (_fileChunks.Any(chunk => chunk.Any()))
            {
                Thread.Sleep(100);
            }
        }
    }
}