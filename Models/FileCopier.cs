using EasySave.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

public class FileCopier
{
    private int filesCopied = 0;
    private Logger logger = new Logger();
    public int FilesCopied { get => filesCopied; }

    public void CopyDirectory(string jobName, string sourceDir, string targetDir, string Type)
    {
        List<string> allFiles = GetAllFiles(sourceDir);
        HashSet<string> loadedHashes = LoadAllowedHashes(jobName);
        HashSet<string> allowedHashes = new HashSet<string>();


        foreach (string file in allFiles)
        {
            if (Type == "différentiel")
            {
                string sourceHash = CalculateMD5(file);
                allowedHashes.Add(sourceHash);
                if (!loadedHashes.Contains(sourceHash))
                {
                    string targetFilePath = Path.Combine(targetDir, Path.GetFileName(file));
                    File.Copy(file, targetFilePath, true);
                    filesCopied++;
                    Console.WriteLine($"Fichier copié avec succès vers {targetFilePath}");
                    logger.LogAction($"Copied file: {file} to {targetFilePath}");
                }
                else
                {
                    Console.WriteLine("Impossible de copier, déjà présent");
                    logger.LogAction($"Skipped copying file: {file}");
                }
                DeleteAllowedHashes(jobName);
                SaveAllowedHashes(allowedHashes, jobName);
            }
            else
            {
                string targetFilePath = Path.Combine(targetDir, Path.GetFileName(file));
                File.Copy(file, targetFilePath, false);
                filesCopied++;
                Console.WriteLine($"Fichier copié avec succès vers {targetFilePath}");
            }
        }
    }

    private List<string> GetAllFiles(string directory)
    {
        List<string> files = new List<string>();
        files.AddRange(Directory.GetFiles(directory));
        string[] subdirectories = Directory.GetDirectories(directory);

        foreach (string subdir in subdirectories)
        {
            files.AddRange(GetAllFiles(subdir));
        }

        return files;
    }

    private string CalculateMD5(string filePath)
    {
        using (var md5 = MD5.Create())
        {
            using (var stream = File.OpenRead(filePath))
            {
                byte[] hash = md5.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
    }
    static HashSet<string> LoadAllowedHashes(string jobName)
    {
        string filePath = string.Format(@"C:\temp\{0}-AlreadyCopiedHashes.json", jobName);


        // Créer un HashSet pour stocker les hachages
        HashSet<string> allowedHashes = new HashSet<string>();

        // Vérifier si le fichier JSON existe
        if (File.Exists(filePath))
        {
            // Lire le fichier JSON ligne par ligne
            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    // Ajouter chaque hachage à la liste
                    allowedHashes.Add(line);
                }
            }
        }

        return allowedHashes;
    }
    static void SaveAllowedHashes(HashSet<string> allowedHashes, string jobName)
    {
        string filePath = string.Format(@"C:\temp\{0}-AlreadyCopiedHashes.json", jobName);

        // Écrire chaque hachage dans le fichier JSON
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            foreach (string hash in allowedHashes)
            {
                writer.WriteLine(hash);
            }
        }
    }
    private void DeleteAllowedHashes(string jobName)
    {
        string filePath = string.Format(@"C:\temp\{0}-AlreadyCopiedHashes.json", jobName);
        File.Delete(filePath);
    }
}
