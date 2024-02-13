using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EasySave.Models
{
    public class Logger
    {
        private readonly string _path = Program.LogsDirectoryPath;
        private static Logger instance;

        private Logger()
        {

            if (!Directory.Exists(_path))
            {
                Directory.CreateDirectory(_path);
            }

            string logPath = _path + @"\log_journalier.json";
            string statePath = _path + @"\state.json";
            if (!File.Exists(logPath))
            {
                File.Create(logPath);
            }
            if (!File.Exists(statePath))
            {
                File.Create(statePath).Close();
            }
        }
        
        public static Logger GetInstance()
        {
            if (instance == null)
            {
                instance = new Logger();
            }
            return instance;
        }

        public void LogAction(string name, string fileSource, string fileTarget, long fileSize, TimeSpan fileTransferTime)
        {
            string logMessage = $"{{\n" +
                                $" \"Name\": \"{name}\",\n" +
                                $" \"FileSource\": \"{fileSource}\",\n" +
                                $" \"FileTarget\": \"{fileTarget}\",\n" +
                                $" \"FileSize\": {fileSize},\n" +
                                $" \"FileTransferTime\": {fileTransferTime},\n" +
                                $" \"Time\": \"{DateTime.Now:dd/MM/yyyy HH:mm:ss}\"\n" +
                                $" }}";
            File.AppendAllText(_path + @"\log_journalier.json", logMessage + Environment.NewLine);
        }

        public void LogState(string name, string sourcePath, string targetPath, JobState state, long nbFileToCopy,
            long fileSize, long nbFileLeftToDo, int progression)
        {
            
            string statePath = _path + @"\state.json";

            sourcePath =  sourcePath.Replace("\\", "\\\\");
            targetPath = targetPath.Replace("\\", "\\\\");
            
            string jsonContent = File.ReadAllText(statePath);
            
            if (jsonContent != string.Empty)
            {
                // Convertir la chaîne JSON en un tableau de JObjects
                JArray jsonArray = JArray.Parse(jsonContent);

                // Trouver l'objet lié à name
                JObject jobEntry = jsonArray.Children<JObject>()
                    .FirstOrDefault(obj => obj["Name"]?.ToString() == name);

                // Vérifier si l'objet existe
                if (jobEntry != null)
                {
                    // Mettre à jour les valeurs de l'objet 
                    jobEntry["SourceFilePath"] = sourcePath;
                    jobEntry["TargetFilePath"] = targetPath;
                    jobEntry["State"] = state.ToString();
                    jobEntry["TotalFilesToCopy"] = nbFileToCopy;
                    jobEntry["TotalFilesSize"] = fileSize;
                    jobEntry["NbFilesLeftToDo"] = nbFileLeftToDo;
                    jobEntry["Progression"] = progression;

                    // Enregistrer les modifications dans le fichier JSON
                    File.WriteAllText(statePath, JsonConvert.SerializeObject(jsonArray, Formatting.Indented));
                }
                else
                {
                    string logMessage = $"{{\n" +
                                        $" \"Name\": \"{name}\",\n" +
                                        $" \"SourceFilePath\": \"{sourcePath}\",\n" +
                                        $" \"TargetFilePath\": \"{targetPath}\",\n" +
                                        $" \"State\": \"{state}\",\n" +
                                        $" \"TotalFilesToCopy\": {nbFileToCopy},\n" +
                                        $" \"TotalFilesSize\": {fileSize},\n" +
                                        $" \"NbFilesLeftToDo\": {nbFileLeftToDo},\n" +
                                        $" \"Progression\": {progression}\n" +
                                        $" }}";
                    
                    File.AppendAllText(_path + @"\state.json", logMessage + Environment.NewLine);                }
            }
            else
            {
                string logMessage = $"[{{\n" +
                                    $" \"Name\": \"{name}\",\n" +
                                    $" \"SourceFilePath\": \"{sourcePath}\",\n" +
                                    $" \"TargetFilePath\": \"{targetPath}\",\n" +
                                    $" \"State\": \"{state}\",\n" +
                                    $" \"TotalFilesToCopy\": {nbFileToCopy},\n" +
                                    $" \"TotalFilesSize\": {fileSize},\n" +
                                    $" \"NbFilesLeftToDo\": {nbFileLeftToDo},\n" +
                                    $" \"Progression\": {progression}, \n" +
                                    $" }}";

                File.AppendAllText(_path + @"\state.json", logMessage + Environment.NewLine);
            }





            /*

           List<JObject> logEntries;

           if (File.Exists(statePath))
           {
               string jsonContent = File.ReadAllText(statePath);
               logEntries = JsonConvert.DeserializeObject<List<JObject>>(jsonContent);
           }
           else
           {
               logEntries = new List<JObject>();
           }

           // Trouver le dictionnaire correspondant au travail de sauvegarde avec le nom spécifié
           JObject jobEntry = logEntries.Find(entry => entry["Name"].ToString() == name);

           if (jobEntry != null)
           {
               jobEntry["SourceFilePath"] = sourcePath;
               jobEntry["TargetFilePath"] = targetPath;
               jobEntry["State"] = state.ToString();
               jobEntry["TotalFilesToCopy"] = nbFileToCopy;
               jobEntry["TotalFilesSize"] = fileSize;
               jobEntry["NbFilesLeftToDo"] = nbFileLeftToDo;
               jobEntry["Progression"] = progression;
           }
           else
           {
               // Créer un nouveau dictionnaire pour le travail de sauvegarde
               jobEntry = new JObject();
               jobEntry["Name"] = name;
               jobEntry["SourceFilePath"] = sourcePath;
               jobEntry["TargetFilePath"] = targetPath;
               jobEntry["State"] = state.ToString();
               jobEntry["TotalFilesToCopy"] = nbFileToCopy;
               jobEntry["TotalFilesSize"] = fileSize;
               jobEntry["NbFilesLeftToDo"] = nbFileLeftToDo;
               jobEntry["Progression"] = progression;

               // Ajouter le nouveau dictionnaire à la liste
               logEntries.Add(jobEntry);
               */
        }

        public void DisplayLog()
        {
            string logContents = File.ReadAllText(_path);
            Console.WriteLine(logContents);
        }
    }
}

