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
    public int FilesCopied { get => filesCopied; }

    public void CopyDirectory(string jobName, string sourceDir, string targetDir, string Type)
    {
        List<string> allFiles = GetAllFiles(sourceDir);
        HashSet<string> loadedHashes = LoadAllowedHashes(jobName);
        HashSet<string> allowedHashes = new HashSet<string>();

        if (Type == "différentiel")
        {
            // Comparer les dossiers dans la destination avec les dossiers dans la source
            foreach (string file in allFiles)
            {

                string sourceHash = CalculateMD5(file);
                allowedHashes.Add(sourceHash);
                if (!loadedHashes.Contains(sourceHash))
                {
                    string relativePath = GetRelativePath(sourceDir, file);
                    string targetFilePath = Path.Combine(targetDir, relativePath);

                    // Créer les sous-dossiers dans la destination si nécessaire
                    string targetFileDir = Path.GetDirectoryName(targetFilePath);
                    if (!Directory.Exists(targetFileDir))
                    {
                        Directory.CreateDirectory(targetFileDir);
                    }

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
            CompareAndDeleteDirectories(targetDir, sourceDir);
        }
        else
        {
            CleanTarget(targetDir);
            foreach (string file in allFiles)
            {
                string relativePath = GetRelativePath(sourceDir, file);
                string targetFilePath = Path.Combine(targetDir, relativePath);
                // Créer les sous-dossiers dans la destination si nécessaire
                string targetFileDir = Path.GetDirectoryName(targetFilePath);
                if (!Directory.Exists(targetFileDir))
                {
                    Directory.CreateDirectory(targetFileDir);
                }

                File.Copy(file, targetFilePath, true);
                filesCopied++;
                Console.WriteLine($"Fichier copié avec succès vers {targetFilePath}");
            }
        }
    }

    private string GetRelativePath(string sourceDir, string filePath)
    {
        return filePath.Substring(sourceDir.Length + 1); // +1 pour enlever le séparateur de dossier
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
    private void CleanTarget(string targetDir) 
    {
        try
        {
            if (Directory.Exists(targetDir))
            {
                // Obtient tous les fichiers dans le dossier
                string[] files = Directory.GetFiles(targetDir);

                // Supprime tous les fichiers du dossier
                foreach (string file in files)
                {
                    File.Delete(file);
                }

                // Obtient tous les sous-dossiers dans le dossier
                string[] subdirectories = Directory.GetDirectories(targetDir);

                // Supprime récursivement le contenu de chaque sous-dossier
                foreach (string subdir in subdirectories)
                {
                    CleanTarget(subdir);
                }

                // Supprime le dossier lui-même après avoir vidé son contenu
                Directory.Delete(targetDir);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Une erreur s'est produite : {ex.Message}");
        }
    }
    private void CompareAndDeleteDirectories(string targetDir, string sourceDir)
    {
        string[] destinationItems = Directory.GetFileSystemEntries(targetDir, "*", SearchOption.AllDirectories);
        string[] sourceItems = Directory.GetFileSystemEntries(sourceDir, "*", SearchOption.AllDirectories);

        // Convertir les chemins en chemins relatifs par rapport au dossier racine
        string[] relativeDestinationItems = Array.ConvertAll(destinationItems, item => GetRelativePath(targetDir, item));
        string[] relativeSourceItems = Array.ConvertAll(sourceItems, item => GetRelativePath(sourceDir, item));

        // Créer des ensembles pour une recherche rapide
        HashSet<string> sourceSet = new HashSet<string>(relativeSourceItems);

        foreach (string destItem in relativeDestinationItems)
        {
            string sourceItem = Path.Combine(sourceDir, destItem);

            if (!sourceSet.Contains(destItem))
            {
                // L'élément de destination n'existe pas dans les sources, supprimer l'élément
                string fullPath = Path.Combine(targetDir, destItem);
                if (File.Exists(fullPath))
                {
                    // Supprimer le fichier
                    File.Delete(fullPath);
                    Console.WriteLine($"Fichier supprimé : {fullPath}");
                }
                else if (Directory.Exists(fullPath))
                {
                    // Supprimer le dossier
                    Directory.Delete(fullPath, true); // Supprimer récursivement
                    Console.WriteLine($"Dossier supprimé : {fullPath}");
                }
            }
        }
    }





}
