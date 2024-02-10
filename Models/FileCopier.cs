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

    public void CopyDirectory(string jobName, string sourceDir, string targetDir, string Type)
    {
        List<string> allFiles = fileGetter.GetAllFiles(sourceDir);
        HashSet<string> loadedHashes = identity.LoadAllowedHashes(jobName);
        HashSet<string> allowedHashes = new HashSet<string>();

        if (Type == "différentiel")
        {
            // Comparer les dossiers dans la destination avec les dossiers dans la source
            foreach (string file in allFiles)
            {

                string sourceHash = identity.CalculateMD5(file);
                allowedHashes.Add(sourceHash);
                if (!loadedHashes.Contains(sourceHash))
                {
                    string relativePath = fileGetter.GetRelativePath(sourceDir, file);
                    string targetFilePath = Path.Combine(targetDir, relativePath);

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
                identity.DeleteAllowedHashes(jobName);
                identity.SaveAllowedHashes(allowedHashes, jobName);
            }
            fileGetter.CompareAndDeleteDirectories(targetDir, sourceDir);
        }
        else
        {
            fileGetter.CleanTarget(targetDir);
            foreach (string file in allFiles)
            {
                string relativePath = fileGetter.GetRelativePath(sourceDir, file);
                string targetFilePath = Path.Combine(targetDir, relativePath);
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
        }
    }
}
