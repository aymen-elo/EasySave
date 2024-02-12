using EasySave.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Resources;
using System.Security.Cryptography;

public class FileCopier
{
    private Logger logger = new Logger();
    private readonly IdentityManager identity = new IdentityManager();
    private FileGetter fileGetter = new FileGetter();

    public void CopyDirectory(Job job)
    {
        // Démarre minuteur
        Stopwatch stopWatch = new Stopwatch();
        stopWatch.Start();
        job.StartTime = DateTime.Now;

        List<string> allFiles = fileGetter.GetAllFiles(job.Source);
        HashSet<string> loadedHashes = identity.LoadAllowedHashes(job.BackupName);
        HashSet<string> allowedHashes = new HashSet<string>();

        job.NbTotalFiles = allFiles.Count();
        if (job.BackupType == BackupType.Diff)
        {
            // Comparer les dossiers dans la destination avec les dossiers dans la source
            foreach (string file in allFiles)
            {

                string sourceHash = identity.CalculateMD5(file);
                allowedHashes.Add(sourceHash);
                if (!loadedHashes.Contains(sourceHash))
                {
                    string relativePath = fileGetter.GetRelativePath(job.Source, file);
                    string targetFilePath = Path.Combine(job.Destination, relativePath);

                    // Créer les sous-dossiers dans la destination si nécessaire
                    string targetFileDir = Path.GetDirectoryName(targetFilePath);
                    if (!Directory.Exists(targetFileDir))
                    {
                        Directory.CreateDirectory(targetFileDir);
                    }
                    File.Copy(file, targetFilePath, true);
                    job.NbSavedFiles++;
                    logger.LogAction($"Copied file: {file} to {targetFilePath}");
                    job.State = JobState.Active;
                }
                else
                {
                    logger.LogAction($"FIle already exists: {file}");
                }
                identity.DeleteAllowedHashes(job.BackupName);
                identity.SaveAllowedHashes(allowedHashes, job.BackupName);
            }
            fileGetter.CompareAndDeleteDirectories(job.Destination, job.Source);
        }
        else
        {
            fileGetter.CleanTarget(job.Destination);
            foreach (string file in allFiles)
            {
                string sourceHash = identity.CalculateMD5(file);
                allowedHashes.Add(sourceHash);
                string relativePath = fileGetter.GetRelativePath(job.Source, file);
                string targetFilePath = Path.Combine(job.Destination, relativePath);
                // Créer les sous-dossiers dans la destination si nécessaire
                string targetFileDir = Path.GetDirectoryName(targetFilePath);
                if (!Directory.Exists(targetFileDir))
                {
                    Directory.CreateDirectory(targetFileDir);
                }

                File.Copy(file, targetFilePath, true);
                job.NbSavedFiles++;
                logger.LogAction($"Copied file: {file} to {targetFilePath}");
                job.State = JobState.Active;
            }
            identity.SaveAllowedHashes(allowedHashes, job.BackupName);
        }
        job.EndTime = DateTime.Now;
        stopWatch.Stop();
        job.Duration = stopWatch.Elapsed;
        Console.WriteLine(job.Duration);
        job.State = JobState.Finished;
    }
}
