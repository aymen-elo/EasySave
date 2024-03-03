using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EasySaveGUI.View;
using EasySaveLib.Model;
using EasySaveLib.Model.CopyHelper;

namespace EasySaveGUI.Helper
{
    /// <summary>
    /// Class responsible for controlling the copying of files and directories.
    /// </summary>
    public class CopyController
    {
        private readonly Logger _logger;
        private readonly object _loggerLock = new();
        private readonly IdentityManager _identity;
        public readonly FileGetter _fileGetter;
        private static int _cleanTargetCalled;
        private static readonly ManualResetEvent CleanTargetDone = new(false);
        private readonly ProgressBar _progressBar;

        // Lists for categorizing files based on priority and size
        private readonly List<string>? _priorityFiles;
        private readonly List<string>? _priorityBigFile;
        private readonly List<string>? _bigFiles;
        private readonly List<string>? _otherFiles;

        private readonly List<string>? _priorityExtensionList;
        private readonly List<string>? _processList;
        
        /* Multi threading related fields */
        /* Separation of a file list into
         ThreadCount number of chunks */
        private readonly List<List<string>> _fileChunks;
        private readonly object _jobLock = new();
        private readonly object _jobProgressionLock = new();
        private readonly object _md5Lock = new();
        private readonly ConcurrentDictionary<string, Task> _filesBeingProcessed = new();
        private int ThreadCount = 1;

        public CopyController()
        {
            _logger = Logger.GetInstance();
            _identity = new IdentityManager();
            _fileGetter = new FileGetter();
            _progressBar = new ProgressBar();
            _fileChunks = new List<List<string>>();
            _priorityFiles = new List<string>();
            _priorityExtensionList = new List<string>();
            _priorityBigFile = new List<string>();
            _bigFiles = new List<string>();
            _otherFiles = new List<string>();
            _processList = ConfigManager.GetProcessList()?.Split(',').ToList();
        }

        /// <summary>
        /// Copies a directory based on the provided Job including multi threading and priorities.
        /// </summary>
        /// <param name="job">The job containing the details of the copy operation.</param>
        public void CopyDirectory(Job job)
        {
            job.StartTime = DateTime.Now;

            var ciphList = new List<string>();
            var stringCipherList = ConfigManager.GetCipherList();

            if (!string.IsNullOrEmpty(stringCipherList))
            {
                var extensionsArray = stringCipherList.Split(',');

                foreach (var extension in extensionsArray)
                {
                    var trimmedExtension = extension.Trim();
                    ciphList.Add(trimmedExtension);
                }
            }

            var _cryptoSoftCipher = new CryptoSoftCipher();

            List<string> allFiles = _fileGetter.GetAllFiles(job.SourceFilePath);
            HashSet<string> loadedHashes = _identity.LoadAllowedHashes(job.Name);
            var allowedHashes = new ConcurrentDictionary<string, byte>();

            job.TotalFilesToCopy = allFiles.Count();

            Categorize(allFiles);

            var stopWatch = new Stopwatch();
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

            _cleanTargetCalled = 0;
            CleanTargetDone.Reset();
        }

        /// <summary>
        /// Categorizes files based on their priority and size.
        /// </summary>
        /// <param name="allFiles">List of all files to be categorized.</param>
        private void Categorize(List<string> allFiles)
        {
            _priorityFiles.Clear();
            _priorityBigFile.Clear();
            _otherFiles.Clear();
            _bigFiles.Clear();

            var stringPriorityList = ConfigManager.GetPriorityList();
            if (!string.IsNullOrEmpty(stringPriorityList))
            {
                var extensionsArray = stringPriorityList.Split(',');

                foreach (var extension in extensionsArray)
                {
                    var trimmedExtension = extension.Trim();
                    _priorityExtensionList.Add(trimmedExtension);
                }
            }

            foreach (var file in allFiles)
            {
                var fileSize = ConfigManager.GetBigFileSize();
                long.TryParse(fileSize, out var longFileSize);
                longFileSize *= 1000;

                if (_priorityExtensionList.Contains(Path.GetExtension(file)) &&
                    longFileSize <= new FileInfo(file).Length)
                    _priorityBigFile.Add(file);
                else if (_priorityExtensionList.Contains(Path.GetExtension(file)))
                    _priorityFiles.Add(file);
                else if (longFileSize <= new FileInfo(file).Length)
                    _bigFiles.Add(file);
                else
                    _otherFiles.Add(file);
            }
        }

        /// <summary>
        /// Distributes for the available threads and
        /// copies files based on their priority and size.
        /// </summary>
        /// <param name="job">The job containing the details of the copy operation.</param>
        /// <param name="allowedHashes">Concurrent dictionary of allowed hashes.</param>
        /// <param name="loadedHashes">Hashset of loaded hashes.</param>
        /// <param name="_cryptoSoftCipher">Instance of CryptoSoftCipher for encryption.</param>
        /// <param name="ciphList">List of file extensions to be encrypted.</param>
        private void DistributeAndCopy(Job job, ConcurrentDictionary<string, byte> allowedHashes,
            HashSet<string> loadedHashes,
            CryptoSoftCipher _cryptoSoftCipher, List<string> ciphList)
        {
            /************************ Priority Big + Priority ****************************/
            /* Priority files - Big and normal TODO : Allocate more threads where there are more files */
            var priorityBigFileChunksCount = _priorityBigFile.Count + _priorityFiles.Count != 0
                ? (int)Math.Ceiling((double)ThreadCount * _priorityBigFile.Count /
                                    (_priorityBigFile.Count + _priorityFiles.Count))
                : 0;
            var priorityFileChunksCount = ThreadCount - priorityBigFileChunksCount;

            var priorityBigFileChunks = ChunkFiles(_priorityBigFile, priorityBigFileChunksCount);
            var priorityFileChunks = ChunkFiles(_priorityFiles, priorityFileChunksCount);

            _fileChunks.AddRange(priorityBigFileChunks);
            _fileChunks.AddRange(priorityFileChunks);

            foreach (var chunk in _fileChunks)
                ThreadPool.QueueUserWorkItem(state =>
                {
                    if (job.BackupType == BackupType.Diff)
                        CopyDiff(job, chunk, allowedHashes, loadedHashes, _cryptoSoftCipher, ciphList);
                    else
                        CopyFull(job, chunk, allowedHashes, _cryptoSoftCipher, ciphList);
                });
            WaitForThreadPoolCompletion();
            _fileChunks.Clear();

            /************************ Big + others (not on priority) ****************************/
            /* Others - Big and normal TODO : Allocate more threads where therae are more files */
            var bigFileChunksCount = _bigFiles.Count + _otherFiles.Count != 0
                ? (int)Math.Ceiling((double)ThreadCount * _bigFiles.Count / (_bigFiles.Count + _otherFiles.Count))
                : 0;
            var otherFileChunksCount = ThreadCount - bigFileChunksCount;

            if (otherFileChunksCount == 0 && _otherFiles.Count != 0)
            {
                ThreadCount = 2;
                otherFileChunksCount = 1;
            }


            var bigFileChunks = ChunkFiles(_bigFiles, bigFileChunksCount);
            var otherFileChunks = ChunkFiles(_otherFiles, otherFileChunksCount);

            _fileChunks.AddRange(bigFileChunks);
            _fileChunks.AddRange(otherFileChunks);
            foreach (var chunk in _fileChunks)
                ThreadPool.QueueUserWorkItem(state =>
                {
                    if (job.BackupType == BackupType.Diff)
                        CopyDiff(job, chunk, allowedHashes, loadedHashes, _cryptoSoftCipher, ciphList);
                    else
                        CopyFull(job, chunk, allowedHashes, _cryptoSoftCipher, ciphList);
                });
            WaitForThreadPoolCompletion();
            _fileChunks.Clear();
        }

        private void CopyDiff(Job job, List<string> chunk, ConcurrentDictionary<string, byte> allowedHashes,
            HashSet<string> loadedHashes, CryptoSoftCipher _cryptoSoftCipher, List<string> cipherList)
        {
            var diSource = new DirectoryInfo(job.SourceFilePath);
            var totalFilesSize = _fileGetter.DirSize(diSource);

            var encryptionKey = ConfigManager.GetCipherList();

            foreach (var file in chunk)
            {
                var copyTime = new Stopwatch();
                copyTime.Start();

                string sourceHash;
                lock (_md5Lock)
                {
                    sourceHash = _identity.CalculateMD5(file);
                }

                allowedHashes.TryAdd(sourceHash, 0);

                if (!loadedHashes.Contains(sourceHash))
                {
                    var relativePath = _fileGetter.GetRelativePath(job.SourceFilePath, file);
                    var targetFilePath = Path.Combine(job.TargetFilePath, relativePath);

                    var targetFileDir = Path.GetDirectoryName(targetFilePath);
                    if (!Directory.Exists(targetFileDir))
                        Directory.CreateDirectory(targetFileDir);

                    // Create a new Mutex for the file
                    var mutex = new Mutex(false, file.Replace('\\', '_'));

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
                job.TotalFilesToCopy, totalFilesSize, job.TotalFilesToCopy - job.NbSavedFiles,
                job.NbSavedFiles * 100 / job.TotalFilesToCopy, job.Name);

            _identity.DeleteAllowedHashes(job.Name);
            _identity.SaveAllowedHashes(allowedHashes.Keys, job.Name);
            _fileGetter.CompareAndDeleteDirectories(job.TargetFilePath, job.SourceFilePath);
        }

        private void CopyFull(Job job, List<string> chunk, ConcurrentDictionary<string, byte> allowedHashes,
            CryptoSoftCipher _cryptoSoftCipher, List<string> cipherList)
        {
            var diSource = new DirectoryInfo(job.SourceFilePath);
            var totalFilesSize = _fileGetter.DirSize(diSource);

            // Clean target but only once by one thread
            if (Interlocked.CompareExchange(ref _cleanTargetCalled, 1, 0) == 0)
            {
                // TODO : Warn about wiping off the Target
                _fileGetter.CleanTarget(job.TargetFilePath);
                CleanTargetDone.Set();
            }

            CleanTargetDone.WaitOne();

            var encryptionKey = ConfigManager.GetEncryptionKey();

            foreach (var file in chunk)
            {
                var copyTime = new Stopwatch();
                copyTime.Start();
                string sourceHash;
                lock (_md5Lock)
                {
                    sourceHash = _identity.CalculateMD5(file);
                }

                allowedHashes.TryAdd(sourceHash, 0);

                var relativePath = _fileGetter.GetRelativePath(job.SourceFilePath, file);
                var targetFilePath = Path.Combine(job.TargetFilePath, relativePath);

                var targetFileDir = Path.GetDirectoryName(targetFilePath);
                if (!Directory.Exists(targetFileDir))
                    Directory.CreateDirectory(targetFileDir);

                var cipherTime = 0;

                if (cipherList.Contains(Path.GetExtension(file)))
                {
                    // Add the file to the dictionary before sending it to CryptoSoft.
                    var encryptionTask = Task.Run(() =>
                    {
                        var cipheTimeString = _cryptoSoftCipher.SendToCryptoSoft(file, targetFileDir, encryptionKey);
                        cipherTime = int.Parse(cipheTimeString);
                    });
                    _filesBeingProcessed.TryAdd(file, encryptionTask);

                    // Wait for the encryption to finish before calling EndFileCopy
                    encryptionTask.ContinueWith(t =>
                    {
                        EndFileCopy(job, targetFilePath + ".cry", file, copyTime, totalFilesSize, cipherTime);
                    });

                    continue;
                }

                File.Copy(file, targetFilePath, true);


                EndFileCopy(job, targetFilePath, file, copyTime, totalFilesSize);

                if (MainWindow.IsStopped)
                    break;
            }

            chunk.Clear();

            _logger.LogState(job.Name, job.BackupType, job.SourceFilePath, job.TargetFilePath, job.State,
                job.TotalFilesToCopy, totalFilesSize, job.TotalFilesToCopy - job.NbSavedFiles,
                job.NbSavedFiles * 100 / job.TotalFilesToCopy, job.Name);

            _identity.DeleteAllowedHashes(job.Name);
            _identity.SaveAllowedHashes(allowedHashes.Keys, job.Name);
        }

        /// <summary>
        /// Executed at the end of each file copy operation to log the result and update the job's progression.
        /// </summary>
        /// <param name="job"></param>
        /// <param name="targetFilePath"></param>
        /// <param name="file"></param>
        /// <param name="copyTime"></param>
        /// <param name="totalFilesSize"></param>
        /// <param name="cipherTime"></param>
        private void EndFileCopy(Job job, string targetFilePath, string file, Stopwatch copyTime, long totalFilesSize,
            int cipherTime = 0)
        {
            // Wait for CryptoSoft to finish processing the file, if necessary.
            if (_filesBeingProcessed.TryGetValue(file, out var task))
            {
                task.Wait();
                _filesBeingProcessed.TryRemove(file, out _);
            }

            lock (_jobLock)
            {
                job.NbSavedFiles++;
                job.NbFilesLeftToDo--;
            }

            ProcessBL.IsProcessRunning(_processList);

            while (MainWindow.IsPaused || ProcessBL.IsDetected)
            {
                Thread.Sleep(100);
                ProcessBL.IsProcessRunning(_processList);
            }

            // TODO : JobID /!\

            Console.WriteLine(_progressBar.UpdateProgress(0, job.Name, job.TotalFilesToCopy, job.NbSavedFiles));

            long fileSize = 0;
            if (File.Exists(targetFilePath)) fileSize = new FileInfo(targetFilePath).Length;

            lock (_loggerLock)
            {
                _logger.LogAction(job.Name, file, targetFilePath, fileSize, copyTime.Elapsed, cipherTime);
            }

            lock (_jobProgressionLock)
            {
                job.Progression = job.NbSavedFiles * 100 / job.TotalFilesToCopy;
            }

            lock (_loggerLock)
            {
                _logger.LogState(job.Name, job.BackupType, job.SourceFilePath, job.TargetFilePath, job.State,
                    job.TotalFilesToCopy, totalFilesSize, job.TotalFilesToCopy - job.NbSavedFiles,
                    job.NbSavedFiles * 100 / job.TotalFilesToCopy, job.Name);
            }

            copyTime.Stop();
            copyTime.Reset();
        }
        
        /// <summary>
        /// Splits a list of strings into a specified number of smaller lists or chunks.
        /// </summary>
        /// <param name="files">The list of strings to be divided into chunks.</param>
        /// <param name="chunkCount">The number of chunks to divide the list into.</param>
        /// <returns>A list of string lists, each representing a chunk of the original list.</returns>
        /// <remarks>
        /// The method calculates the size of each chunk by dividing the total number of files by the desired number of chunks, rounding up if necessary to ensure all files are included in the chunks.
        /// It then iterates over the list of files, creating a new chunk for each group of files of the calculated chunk size, and adds these chunks to the list of chunks.
        /// If the total number of files is not evenly divisible by the chunk size, the last chunk will contain the remaining files and may be smaller than the other chunks.
        /// </remarks>
        private List<List<string>> ChunkFiles(List<string> files, int chunkCount)
        {
            var chunks = new List<List<string>>();
            var chunkSize = (int)Math.Ceiling((double)files.Count / chunkCount);

            for (var i = 0; i < files.Count; i += chunkSize)
                chunks.Add(files.GetRange(i, Math.Min(chunkSize, files.Count - i)));

            return chunks;
        }

        private void WaitForThreadPoolCompletion()
        {
            while (_fileChunks.Any(chunk => chunk.Any())) Thread.Sleep(100);
        }
    }
}