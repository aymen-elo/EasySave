﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace EasySaveLib.Model
{
    public class IdentityManager
    {
        private readonly string _logDirPath = Logger.LogsDirectoryPath;
        
        /* Lock for hash saving because of multi threading */
        private static readonly object Lock = new object();
        
        public IdentityManager()
        {
            if (!Directory.Exists(_logDirPath))
            {
                Directory.CreateDirectory(_logDirPath);
            }
        }
        
        public string CalculateMD5(string filePath)
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
        public HashSet<string> LoadAllowedHashes(string jobName)
        {
            var filePath = _logDirPath + string.Format( @"\{0}-AlreadyCopiedHashes.json", jobName);
            
            // Créer un HashSet pour stocker les hachages
            HashSet<string> allowedHashes = new HashSet<string>();

            // Vérifier si le fichier JSON existe
            if (File.Exists(filePath))
            {
                // Lire le fichier JSON ligne par ligne
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string? line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        // Ajouter chaque hachage à la liste
                        allowedHashes.Add(line);
                    }
                }
            }

            return allowedHashes;
        }
        public void SaveAllowedHashes(IEnumerable<string> allowedHashes, string jobName)
        {
            string filePath = _logDirPath + string.Format( @"\{0}-AlreadyCopiedHashes.json", jobName);

            lock (Lock)
            {
                // Écrire chaque hachage dans le fichier JSON
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    foreach (string hash in allowedHashes)
                    {
                        writer.WriteLine(hash);
                    }
                }
            }
        }
        public void DeleteAllowedHashes(string jobName)
        {
            string filePath = _logDirPath + string.Format( @"\{0}-AlreadyCopiedHashes.json", jobName);
            File.Delete(filePath);
        }
    }
}
