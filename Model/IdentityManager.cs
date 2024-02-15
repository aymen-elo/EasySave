using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace EasySave.Models
{
    public class IdentityManager
    {
        private readonly string _path = Program.LogsDirectoryPath;
        public IdentityManager()
        {
            if (!Directory.Exists(_path))
            {
                Directory.CreateDirectory(_path);
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
            string filePath = Path.Combine(_path, string.Format( @"\{0}-AlreadyCopiedHashes.json", jobName));
            
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
        public void SaveAllowedHashes(HashSet<string> allowedHashes, string jobName)
        {
            string filePath = _path + string.Format( @"\{0}-AlreadyCopiedHashes.json", jobName);
            
            // Écrire chaque hachage dans le fichier JSON
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                foreach (string hash in allowedHashes)
                {
                    writer.WriteLine(hash);
                }
            }
        }
        public void DeleteAllowedHashes(string jobName)
        {
            string filePath = Path.Combine(_path, string.Format( @"\{0}-AlreadyCopiedHashes.json", jobName));
            File.Delete(filePath);
        }
    }
}
