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
                File.Create(logPath).Close();
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

        public void LogAction(string name, string fileSource, string fileTarget, long fileSize,
            TimeSpan fileTransferTime)
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

            sourcePath = sourcePath.Replace("\\", "\\\\");
            targetPath = targetPath.Replace("\\", "\\\\");

            string jsonContent = File.ReadAllText(statePath);

            if (File.Exists(statePath))
            {
                if (jsonContent != string.Empty)
                {
                    JArray jsonArray = JArray.Parse(jsonContent);
                    JObject jobEntry = jsonArray.Children<JObject>()
                        .FirstOrDefault(obj => obj["Name"]?.ToString() == name);

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
                        JObject newJob = CreateJobObject(name, sourcePath, targetPath, state, nbFileToCopy, fileSize,
                            nbFileLeftToDo, progression);
                        jsonArray.Add(newJob);
                    }

                    File.WriteAllText(statePath, JsonConvert.SerializeObject(jsonArray, Formatting.Indented));
                }
                else
                {
                    JArray jsonArray = new JArray();
                    JObject newJob = CreateJobObject(name, sourcePath, targetPath, state, nbFileToCopy, fileSize,
                        nbFileLeftToDo, progression);
                    jsonArray.Add(newJob);
                    File.WriteAllText(statePath, JsonConvert.SerializeObject(jsonArray, Formatting.Indented));
                }
            }
        }

        private JObject CreateJobObject(string name, string sourcePath, string targetPath, JobState state, long nbFileToCopy,
            long fileSize, long nbFileLeftToDo, int progression)
        {
            JObject jobObject = new JObject();
            jobObject["Name"] = name;
            jobObject["SourceFilePath"] = sourcePath;
            jobObject["TargetFilePath"] = targetPath;
            jobObject["State"] = state.ToString();
            jobObject["TotalFilesToCopy"] = nbFileToCopy;
            jobObject["TotalFilesSize"] = fileSize;
            jobObject["NbFilesLeftToDo"] = nbFileLeftToDo;
            jobObject["Progression"] = progression;
            return jobObject; 
        } 

        public void DisplayLog()
        {
            string logContents = File.ReadAllText(_path);
            Console.WriteLine(logContents);
        }
    }
}

