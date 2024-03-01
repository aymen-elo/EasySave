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
using System.Threading.Tasks;
using System.Windows;
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
        private List<string>? priorityFiles;
        private List<string>? priorityExtensionList;
        private List<string>? priorityBigFile;
        private List<string>? bigFiles;
        private List<string>? otherFiles;
        private List<string>? processList;


        private readonly ProgressBar _progressBar;
        
        /* Multi threading
         DONE : Full Backup
         TODO : Diff Backup */
        /* Separation of a file list into
         ThreadCount number of chunks */
        private List<List<string>> _fileChunks;
        private readonly object _jobLock = new object();
        private readonly object _jobProgressionLock = new object();
        private readonly object _cipherLock = new object();
        private readonly object _md5Lock = new object();
        private ConcurrentDictionary<string, Task> _filesBeingProcessed = new ConcurrentDictionary<string, Task>();
        private const int ThreadCount = 1;

        public CopyController()
        {
            _logger = Logger.GetInstance();
            _identity = new IdentityManager();
            _fileGetter = new FileGetter();
            _progressBar = new ProgressBar();
            _fileChunks = new List<List<string>>();
            priorityFiles = new List<string>();
            priorityExtensionList = new List<string>();
            priorityBigFile = new List<string>();
            bigFiles = new List<string>();
            otherFiles = new List<string>();
            processList = ConfigManager.GetProcessList()?.Split(',').ToList();
        }

        public void CopyDirectory(Job job)
        {
            
            job.StartTime = DateTime.Now;
            
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
            
            Categorize(allFiles);

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            // TODO : JobID /!\
            
            Console.WriteLine(_progressBar.UpdateProgress(0, job.Name, job.TotalFilesToCopy, job.NbSavedFiles));

            /* Handle Copying with multi threading */
            DistributeAndCopy(job, allowedHashes, loadedHashes, _cryptoSoftCipher, ciphList);

            stopWatch.Stop();
            job.Duration = stopWatch.Elapsed;

            job.EndTime = DateTime.Now;
            job.NbFilesLeftToDo = 0;
            job.State = JobState.Finished;
            _logger.LogState(job);
            
            cleanTargetCalled = 0;
            cleanTargetDone.Reset();
        }

        private void Categorize(List<string> allFiles)
        {
            priorityFiles.Clear();
            priorityBigFile.Clear();
            otherFiles.Clear();
            bigFiles.Clear();
            
            string stringPriorityList = ConfigManager.GetPriorityList();
            if (!string.IsNullOrEmpty(stringPriorityList))
            {
                string[] extensionsArray = stringPriorityList.Split(',');

                foreach (string extension in extensionsArray)
                {
                    string trimmedExtension = extension.Trim();
                    priorityExtensionList.Add(trimmedExtension);
                }
            }

            foreach (var file in allFiles)
            {
                string fileSize = ConfigManager.GetBigFileSize();
                long.TryParse(fileSize, out long longFileSize);
                longFileSize *= 1000;
                
                if (priorityExtensionList.Contains(Path.GetExtension(file)) &&
                    (longFileSize <= (new FileInfo(file)).Length))
                {
                    priorityBigFile.Add(file);
                }
                else if (priorityExtensionList.Contains(Path.GetExtension(file)))
                {
                    priorityFiles.Add(file);
                }
                else if (longFileSize >= (new FileInfo(file)).Length)
                {
                    bigFiles.Add(file);
                }
                else
                {
                    otherFiles.Add(file);
                }
            }
        }

        private void DistributeAndCopy(Job job, ConcurrentDictionary<string, byte> allowedHashes, HashSet<string> loadedHashes,
            CryptoSoftCipher _cryptoSoftCipher, List<string> ciphList)
        {
            /************************ Priority Big + Priority ****************************/
            /* Priority files - Big and normal TODO : Allocate more threads where there are more files */
            int priorityBigFileChunksCount = (priorityBigFile.Count + priorityFiles.Count) != 0 ? (int)Math.Ceiling((double)ThreadCount * priorityBigFile.Count / (priorityBigFile.Count + priorityFiles.Count)) : 0;
            int priorityFileChunksCount = ThreadCount - priorityBigFileChunksCount;
            
            var priorityBigFileChunks = ChunkFiles(priorityBigFile, priorityBigFileChunksCount);
            var priorityFileChunks = ChunkFiles(priorityFiles, priorityFileChunksCount);

            _fileChunks.AddRange(priorityBigFileChunks);
            _fileChunks.AddRange(priorityFileChunks);
            
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
            _fileChunks.Clear();

            /************************ Big + others (not on priority) ****************************/
            /* Others - Big and normal TODO : Allocate more threads where therae are more files */
            int bigFileChunksCount = (bigFiles.Count + otherFiles.Count) != 0 ? (int)Math.Ceiling((double)ThreadCount * bigFiles.Count / (bigFiles.Count + otherFiles.Count)) : 0;
            int otherFileChunksCount = ThreadCount - bigFileChunksCount;

            var bigFileChunks = ChunkFiles(bigFiles, bigFileChunksCount);
            var otherFileChunks = ChunkFiles(otherFiles, otherFileChunksCount);

            _fileChunks.AddRange(bigFileChunks);
            _fileChunks.AddRange(otherFileChunks);
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
            _fileChunks.Clear();
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

                string sourceHash;
                lock (_md5Lock)
                {
                    sourceHash = _identity.CalculateMD5(file);
                }
                allowedHashes.TryAdd(sourceHash, 0);

                if (!loadedHashes.Contains(sourceHash))
                {
                    string relativePath = _fileGetter.GetRelativePath(job.SourceFilePath, file);
                    string targetFilePath = Path.Combine(job.TargetFilePath, relativePath);

                    string targetFileDir = Path.GetDirectoryName(targetFilePath);
                    if (!Directory.Exists(targetFileDir))
                        Directory.CreateDirectory(targetFileDir);

                    // Create a new Mutex for the file
                    Mutex mutex = new Mutex(false, file.Replace('\\', '_'));

                    // Wait until it is safe to enter (i.e., until no other thread is in the critical section).
                    mutex.WaitOne();

                    try
                    {
                        File.Copy(file, targetFilePath, true);
                    }
                    finally
                    {
                        // Release the Mutex.
                        mutex.ReleaseMutex();
                    }

                    EndFileCopy(job, targetFilePath, file, copyTime, totalFilesSize);
                }
                else
                {
                    // Log that the file already exists in the target (no need to copy)
                    var helperTargetFilePath = Path.Combine(job.TargetFilePath,
                        _fileGetter.GetRelativePath(job.SourceFilePath, file));
                    _logger.LogAction("Already exists" + job.Name, file, "", 0, TimeSpan.Zero);
                    lock (_loggerLock)
                    {
                        EndFileCopy(job, helperTargetFilePath, file, copyTime, totalFilesSize);
                    }
                }
            }

            chunk.Clear();
            _logger.LogState(job.Name, job.BackupType, job.SourceFilePath, job.TargetFilePath, job.State,
                job.TotalFilesToCopy, totalFilesSize, (job.TotalFilesToCopy - job.NbSavedFiles),
                ((job.NbSavedFiles * 100) / job.TotalFilesToCopy), job.Name);

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
            cleanTargetDone.WaitOne();

            string encryptionKey = ConfigManager.GetEncryptionKey();

            foreach (string file in chunk)
            {
                Stopwatch copyTime = new Stopwatch();
                copyTime.Start();
                string sourceHash;
                lock (_md5Lock)
                {
                    sourceHash = _identity.CalculateMD5(file);
                }
                allowedHashes.TryAdd(sourceHash, 0);

                string relativePath = _fileGetter.GetRelativePath(job.SourceFilePath, file);
                string targetFilePath = Path.Combine(job.TargetFilePath, relativePath);

                string targetFileDir = Path.GetDirectoryName(targetFilePath);
                if (!Directory.Exists(targetFileDir))
                    Directory.CreateDirectory(targetFileDir);


                if (cipherList.Contains(Path.GetExtension(file)))
                {
                    // Add the file to the dictionary before sending it to CryptoSoft.
                    var encryptionTask = Task.Run(() =>
                    {
                        Console.WriteLine(
                            _cryptoSoftCipher.sendToCryptoSoft(file, targetFileDir, encryptionKey));
                    });
                    _filesBeingProcessed.TryAdd(file, encryptionTask);

                    // Wait for the encryption to finish before calling EndFileCopy
                    encryptionTask.ContinueWith(t =>
                    {
                        EndFileCopy(job, targetFilePath + ".cry", file, copyTime, totalFilesSize);
                    });

                    continue;
                }

                File.Copy(file, targetFilePath, true);
                

                EndFileCopy(job, targetFilePath, file, copyTime, totalFilesSize);
                
            }
            chunk.Clear();
            
            _logger.LogState(job.Name, job.BackupType, job.SourceFilePath, job.TargetFilePath, job.State, job.TotalFilesToCopy, totalFilesSize , (job.TotalFilesToCopy - job.NbSavedFiles), ((job.NbSavedFiles * 100) / job.TotalFilesToCopy), job.Name);
            
            _identity.DeleteAllowedHashes(job.Name);
            _identity.SaveAllowedHashes(allowedHashes.Keys, job.Name);
        }
        
        private void EndFileCopy(Job job, string targetFilePath, string file, Stopwatch copyTime, long totalFilesSize)
        {
            // Wait for CryptoSoft to finish processing the file, if necessary.
            if (_filesBeingProcessed.TryGetValue(file, out Task task))
            {
                task.Wait();
                _filesBeingProcessed.TryRemove(file, out _);
            }
            
            lock (_jobLock)
            {
                job.NbSavedFiles++;
                job.NbFilesLeftToDo--;
            }

            // TODO : JobID /!\
            
            Console.WriteLine(_progressBar.UpdateProgress(0, job.Name, job.TotalFilesToCopy, job.NbSavedFiles));
            
            long fileSize = 0;
            if (File.Exists(targetFilePath))
            {
                fileSize = new FileInfo(targetFilePath).Length;
            }
            
            lock (_loggerLock)
            {
                _logger.LogAction(job.Name, file, targetFilePath, fileSize, copyTime.Elapsed);
            }
            
            lock (_jobProgressionLock)
            {
                job.Progression = ((job.NbSavedFiles * 100) / job.TotalFilesToCopy);
            }

            lock (_loggerLock)
            {
                _logger.LogState(job.Name, job.BackupType, job.SourceFilePath, job.TargetFilePath, job.State, job.TotalFilesToCopy, totalFilesSize , (job.TotalFilesToCopy - job.NbSavedFiles), ((job.NbSavedFiles * 100) / job.TotalFilesToCopy), job.Name);   
            }
                    
            copyTime.Stop();
            copyTime.Reset();
            
            //List<string> processBlackList = new List<string>();

            
            //processBlackList.Add("calc");
            //processBlackList.Add("CalculatorApp");
            
            ProcessBL.IsProcessRunning(processList);
            
            while (MainWindow.IsPaused || ProcessBL.IsDetected)
            {  
                Thread.Sleep(100);
                ProcessBL.IsProcessRunning(processList);
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