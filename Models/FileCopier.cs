using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using EasySave.Library;

namespace EasySave.Models
{
    public class FileCopier
    {
        // Définition des instances utiles pour copie
        private Logger _logger = Logger.GetInstance();
        private readonly IdentityManager _identity = new IdentityManager();
        private FileGetter _fileGetter = new FileGetter();

        
        public FileCopier() { }
        public void CopyDirectory(Job job, TranslationModel translation)
        {
            List<string> allFiles = _fileGetter.GetAllFiles(job.Source);
            HashSet<string> loadedHashes = _identity.LoadAllowedHashes(job.BackupName);
            HashSet<string> allowedHashes = new HashSet<string>();
        
            // Stockage valeurs dans le travail en cours 
            job.StartTime = DateTime.Now;
            job.NbTotalFiles = allFiles.Count();
        
            // Copy suivant type de sauvegarde
            if (job.BackupType == BackupType.Diff)
            {
                CopyDiff(job, allFiles, allowedHashes, loadedHashes, translation);
            }
            else
            {
                CopyFull(job, allFiles, allowedHashes, translation);
            }
        
            // Fin Minuteur + stockage des données
            job.EndTime = DateTime.Now;
            job.State = JobState.Finished;
        }

        private void CopyDiff(Job job, List<string> allFiles, HashSet<string> allowedHashes, HashSet<string> loadedHashes, TranslationModel translation)
        {
            
            string message = translation.FileCopier.WarningMessage; //
            bool warningAccepted = CopyWarning(message, translation);
            long savedFileSize = 0;
            
            // Calcul Taille Dir
            DirectoryInfo diSource = new DirectoryInfo(job.Source);
            Console.WriteLine(_fileGetter.DirSize(diSource));

            if (warningAccepted == true)
            {
                // Démarre minuteur
                
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();
                // Comparer les dossiers dans la destination avec les dossiers dans la source
                foreach (string file in allFiles)
                {
                    string sourceHash = _identity.CalculateMD5(file);
                    allowedHashes.Add(sourceHash);
                    if (!loadedHashes.Contains(sourceHash))
                    {
                        string relativePath = _fileGetter.GetRelativePath(job.Source, file);
                        string targetFilePath = Path.Combine(job.Destination, relativePath);

                        // Créer les sous-dossiers dans la destination si nécessaire
                        string targetFileDir = Path.GetDirectoryName(targetFilePath);
                        if (!Directory.Exists(targetFileDir))
                        {
                            Directory.CreateDirectory(targetFileDir);
                        }

                        // Copier le fichier
                        File.Copy(file, targetFilePath, true);
                        job.NbSavedFiles++;

                        // Obtenir la taille du fichier
                        long fileSize = new System.IO.FileInfo(targetFilePath).Length;

                        // Loguer l'action
                        _logger.LogAction(job.BackupName, file, targetFilePath, fileSize, stopWatch.Elapsed);

                        job.State = JobState.Active;
                        Console.WriteLine(fileSize);
                    }
                    else 
                    {
                        _logger.LogAction("Existe déjà" + job.BackupName, file, "", 0, TimeSpan.Zero);

                    }

                    _identity.DeleteAllowedHashes(job.BackupName);
                    _identity.SaveAllowedHashes(allowedHashes, job.BackupName);
                }

                _fileGetter.CompareAndDeleteDirectories(job.Destination, job.Source);
                stopWatch.Stop();
                job.Duration = stopWatch.Elapsed;
                Console.WriteLine(job.Duration);
            }
            else
            {
                return;
            }
        }

        private void CopyFull (Job job, List<string> allFiles, HashSet<string> allowedHashes, TranslationModel translation)
        {
            string message = (translation.FileCopier.WarningMessage); //
            bool warningAccepted = CopyWarning(message, translation);
            if (warningAccepted == true)
            {
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();
                _fileGetter.CleanTarget(job.Destination);
                foreach (string file in allFiles)
                {
                    string sourceHash = _identity.CalculateMD5(file);
                    allowedHashes.Add(sourceHash);

                    string relativePath = _fileGetter.GetRelativePath(job.Source, file);
                    string targetFilePath = Path.Combine(job.Destination, relativePath);

                    // Créer les sous-dossiers dans la destination si nécessaire
                    string targetFileDir = Path.GetDirectoryName(targetFilePath);
                    if (!Directory.Exists(targetFileDir))
                    {
                        Directory.CreateDirectory(targetFileDir);
                    }

                    // Copier le fichier
                    File.Copy(file, targetFilePath, true);
                    job.NbSavedFiles++;

                    // Obtenir la taille du fichier
                    long fileSize = new System.IO.FileInfo(targetFilePath).Length;

                    // Loguer l'action
                    _logger.LogAction(job.BackupName, file, targetFilePath, fileSize, stopWatch.Elapsed);

                    job.State = JobState.Active;
                    _logger.LogState(job.BackupName, job.Source, job.Destination, job.State, job.NbTotalFiles, fileSize, (job.NbTotalFiles - job.NbSavedFiles),((job.NbSavedFiles*100)/job.NbTotalFiles));
                }

                // Sauvegarder les hashes autorisés
                _identity.SaveAllowedHashes(allowedHashes, job.BackupName);

                // Arrêter le minuteur
                stopWatch.Stop();
                job.Duration = stopWatch.Elapsed;
                Console.WriteLine(job.Duration);
            }
            else
            {
                return;
            }
        }

        private bool CopyWarning(string message, TranslationModel translation)
        {
            Console.WriteLine(message);

            // Demander à l'utilisateur de continuer
            Console.Write(translation.FileCopier.Continue); //
            string response = Console.ReadLine();

            // Vérifier la réponse de l'utilisateur
            if (response.ToLower() == "y")
            {
                return true;
            }
            else if (response.ToLower() == "n")
            {
                return false;
            }
            else
            {
                Console.WriteLine(translation.FileCopier.InvalidResponseFileCopier);
                return CopyWarning(message, translation);
            }
        }
    }
}
