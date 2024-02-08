using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

public class FileCopier
{
    private int filesCopied = 0;

    public int FilesCopied { get => filesCopied; }

    public void CopyDirectory(string sourceDir, string targetDir, HashSet<string> allowedHashes, string Type)
    {
        List<string> allFiles = GetAllFiles(sourceDir);

        foreach (string file in allFiles)
        {
            string sourceHash = CalculateMD5(file);

            if (!allowedHashes.Contains(sourceHash) && Type == "différentiel")
            {
                string targetFilePath = Path.Combine(targetDir, Path.GetFileName(file));
                File.Copy(file, targetFilePath, false);
                filesCopied++;
                Console.WriteLine($"Fichier copié avec succès vers {targetFilePath}");
            }
            else
            {
                Console.WriteLine("Impossible de copier, déjà présent");
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
}
