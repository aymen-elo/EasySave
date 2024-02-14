using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic.FileIO;
using SearchOption = System.IO.SearchOption;

namespace EasySave.Models
{
    public class FileGetter
    {
        public string GetRelativePath(string sourceDir, string filePath)
        {
            return filePath.Substring(sourceDir.Length + 1); // +1 pour enlever le séparateur de dossier
        }

        public List<string> GetAllFiles(string directory)
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

        public void CleanTarget(string targetDir)
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
                        FileSystem.DeleteFile(file, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                    }

                    // Obtient tous les sous-dossiers dans le dossier
                    string[] subdirectories = Directory.GetDirectories(targetDir);

                    // Supprime récursivement le contenu de chaque sous-dossier
                    foreach (string subdir in subdirectories)
                    {
                        CleanTarget(subdir);
                    }
                }
            }
            catch (Exception ex)
            {
                // TO DO : ADD LOG
                Console.WriteLine($"Une erreur s'est produite : {ex.Message}");
            }
        }
        public void CompareAndDeleteDirectories(string targetDir, string sourceDir)
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
                        FileSystem.DeleteFile(fullPath, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                    }
                    else if (Directory.Exists(fullPath))
                    {
                        // Supprimer le dossier
                        FileSystem.DeleteDirectory(fullPath, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                    }
                }
            }
        }
        
        public long DirSize(DirectoryInfo dir) 
        {    
            long size = 0;    
            // Add file sizes.
            FileInfo[] fis = dir.GetFiles();
            foreach (FileInfo fi in fis) 
            {      
                size += fi.Length;    
            }
            // Add subdirectory sizes.
            DirectoryInfo[] dis = dir.GetDirectories();
            foreach (DirectoryInfo di in dis) 
            {
                size += DirSize(di);   
            }
            return size;  
        }
    }
}
