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
    private int filesCopied = 0;
    private Logger logger = new Logger();
    private readonly IdentityManager identity = new IdentityManager();
    private FileGetter fileGetter = new FileGetter();
    public int FilesCopied { get => filesCopied; }

    public void CopyDirectory(Job job)
    {
        List<string> allFiles = fileGetter.GetAllFiles(job.Source);
        HashSet<string> loadedHashes = identity.LoadAllowedHashes(job.BackupName);
        HashSet<string> allowedHashes = new HashSet<string>();

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
                    filesCopied++;
                    logger.LogAction($"Copied file: {file} to {targetFilePath}");
                }
                else
                {
                    Console.WriteLine("Impossible de copier, déjà présent");
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
            }
        }
    }
}
