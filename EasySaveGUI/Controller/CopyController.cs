using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Formats.Asn1;
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
        private readonly object _loggerLock = new object();
        private readonly IdentityManager _identity;
        public FileGetter _fileGetter;
        private static int cleanTargetCalled = 0;
        private static ManualResetEvent cleanTargetDone = new ManualResetEvent(false);

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
            
            
            List<string> ciphList = new List<string>();
            string stringCipherList = ConfigManager.GetCipherList();
            
            if (!string.IsNullOrEmpty(stringCipherList))
            {
                string[] extensionsArray = stringCipherList.Split(',');

                foreach (string extension in extensionsArray)
                {
                    string trimmedExtension = extension.Trim();
                    ciphList.Add(trimmedExtension);
                }
            }

            CryptoSoftCipher _cryptoSoftCipher = new CryptoSoftCipher();
            
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
                        CopyDiff(job, chunk, allowedHashes, loadedHashes, _cryptoSoftCipher, ciphList);
                    else
                        CopyFull(job, chunk, allowedHashes, _cryptoSoftCipher, ciphList);
                });
            }
            WaitForThreadPoolCompletion();

            stopWatch.Stop();
            job.Duration = stopWatch.Elapsed;

            job.EndTime = DateTime.Now;
            job.NbFilesLeftToDo = 0;
            job.State = JobState.Finished;
            
        }

        private void CopyDiff(Job job, List<string> chunk, ConcurrentDictionary<string, byte> allowedHashes, HashSet<string> loadedHashes, CryptoSoftCipher _cryptoSoftCipher, List<string> cipherList)
        {
            DirectoryInfo diSource = new DirectoryInfo(job.SourceFilePath);
            long totalFilesSize = _fileGetter.DirSize(diSource);
            
            string encryptionKey = ConfigManager.GetCipherList();

            foreach (string file in chunk)
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
            _logger.LogState(job.Name, job.BackupType, job.SourceFilePath, job.TargetFilePath, job.State, job.TotalFilesToCopy, totalFilesSize , (job.TotalFilesToCopy - job.NbSavedFiles), ((job.NbSavedFiles * 100) / job.TotalFilesToCopy), job.Name);
            
            _identity.DeleteAllowedHashes(job.Name);
            _identity.SaveAllowedHashes(allowedHashes.Keys, job.Name);
            _fileGetter.CompareAndDeleteDirectories(job.TargetFilePath, job.SourceFilePath);
        }

        private void CopyFull(Job job, List<string> chunk, ConcurrentDictionary<string, byte> allowedHashes, CryptoSoftCipher _cryptoSoftCipher, List<string> cipherList)
        {
            DirectoryInfo diSource = new DirectoryInfo(job.SourceFilePath);
            long totalFilesSize = _fileGetter.DirSize(diSource);
            
            // Clean target but only once by one thread
            if (Interlocked.CompareExchange(ref cleanTargetCalled, 1, 0) == 0)
            {
                // TODO : Warn about wiping off the Target
                _fileGetter.CleanTarget(job.TargetFilePath);
                cleanTargetDone.Set();
            }
            else
            {
                cleanTargetDone.WaitOne();
            }

            string encryptionKey = ConfigManager.GetEncryptionKey();

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
                
                if (cipherList.Contains(Path.GetExtension(file)))
                {
                    Console.WriteLine(_cryptoSoftCipher.sendToCryptoSoft(file, targetFileDir, encryptionKey));
                    continue;
                }

                File.Copy(file, targetFilePath, true);

                EndFileCopy(job, targetFilePath, file, copyTime, totalFilesSize);
                
            }
            chunk.Clear();
            
            job.State = JobState.Finished;
            _logger.LogState(job.Name, job.BackupType, job.SourceFilePath, job.TargetFilePath, job.State, job.TotalFilesToCopy, totalFilesSize , (job.TotalFilesToCopy - job.NbSavedFiles), ((job.NbSavedFiles * 100) / job.TotalFilesToCopy), job.Name);
            
            _identity.DeleteAllowedHashes(job.Name);
            _identity.SaveAllowedHashes(allowedHashes.Keys, job.Name);
        }
        
        private void EndFileCopy(Job job, string targetFilePath, string file, Stopwatch copyTime, long totalFilesSize)
        {
            
            job.NbSavedFiles++;

            // TODO : JobID /!\
            
            Console.WriteLine(_progressBar.UpdateProgress(0, job.Name, job.TotalFilesToCopy, job.NbSavedFiles));

            long fileSize = new System.IO.FileInfo(targetFilePath).Length;

            lock (_loggerLock)
            {
                _logger.LogAction(job.Name, file, targetFilePath, fileSize, copyTime.Elapsed);    
            }
            
            job.State = JobState.Active;
            job.Progression = ((job.NbSavedFiles * 100) / job.TotalFilesToCopy);

            lock (_loggerLock)
            {
                _logger.LogState(job.Name, job.BackupType, job.SourceFilePath, job.TargetFilePath, job.State, job.TotalFilesToCopy, totalFilesSize , (job.TotalFilesToCopy - job.NbSavedFiles), ((job.NbSavedFiles * 100) / job.TotalFilesToCopy), job.Name);   
            }
                    
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