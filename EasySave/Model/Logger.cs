using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EasySave.Model
{
    public class Logger
    {
        private readonly string _path = Program.LogsDirectoryPath;
        private static Logger _instance;
        public string _logFormat { get; set; }

        private Logger()
        {
            _logFormat = "json";

            if (!Directory.Exists(_path))
            {
                Directory.CreateDirectory(_path);
            }

            string statePath = Path.Combine(_path, "state.json");
            if (!File.Exists(statePath))
            {
                File.WriteAllText(statePath, ""); 
            }
        }
        
        public static Logger GetInstance()
        {
            if (_instance == null)
            {
                _instance = new Logger();
            }
            return _instance;
        }

        public void LogAction(string name, string fileSource, string fileTarget, long fileSize, TimeSpan fileTransferTime)
        {
            if (_logFormat == "json")
            {
                LogActionJson( name,  fileSource,  fileTarget,  fileSize,  fileTransferTime);
            } else if (_logFormat == "xml")
            {
                LogActionXml(name, fileSource, fileTarget, fileSize, fileTransferTime);
            } else
            {
                LogActionJson( name,  fileSource,  fileTarget,  fileSize,  fileTransferTime);
            }

            
        }

        public void LogActionJson(string name, string fileSource, string fileTarget, long fileSize,
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
            
            string jsonFilePath = Path.Combine(_path, $"{DateTime.Now:yyyy-MM-dd}.json");

            if (!File.Exists(jsonFilePath))
            {
                using (File.Create(jsonFilePath)) { }
            }

            File.AppendAllText(jsonFilePath, logMessage + Environment.NewLine);
        }
        
        
        public void LogActionXml(string name, string fileSource, string fileTarget, long fileSize, TimeSpan fileTransferTime)
        {
            string transferTimeString = fileTransferTime.ToString();

            XElement logElement = new XElement("Log",
                new XElement("Name", name),
                new XElement("FileSource", fileSource),
                new XElement("FileTarget", fileTarget),
                new XElement("FileSize", fileSize),
                new XElement("FileTransferTime", transferTimeString),
                new XElement("Time", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")));

            string xmlFilePath = Path.Combine(_path, $"{DateTime.Now:yyyy-MM-dd}.xml");

            if (!File.Exists(xmlFilePath))
            {
                XDocument newDoc = new XDocument(new XElement("Logs"));
                newDoc.Save(xmlFilePath);
            }

            XDocument doc = XDocument.Load(xmlFilePath);
            doc.Element("Logs")?.Add(logElement);
            doc.Save(xmlFilePath);
        }

        
        /* Logging inside state.json, newName is for EditJob (search done by name, hence the need for both old & new names) */
        public void LogState(string name, string sourcePath, string targetPath, JobState state, long nbFileToCopy,
            long fileSize, long nbFileLeftToDo, int progression, string newName)
        {
            string statePath = _path + @"\state.json";
            
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
                        jobEntry["Name"] = newName;
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


                /* method for logs in XML */
               LogStateXml(name, sourcePath, targetPath, state, nbFileToCopy, fileSize, nbFileLeftToDo, progression);

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
        
        public void LogStateXml(string name, string sourcePath, string targetPath, JobState state, long nbFileToCopy,
            long fileSize, long nbFileLeftToDo, int progression)
        {
            // Chemin du fichier XML de log d'état
            string xmlFilePath = _path + @"\state.xml";

            // Vérification de l'existence du fichier XML
            if (!File.Exists(xmlFilePath) || new FileInfo(xmlFilePath).Length == 0)
            {
                // Si le fichier n'existe pas ou s'il est vide, créez un nouvel élément racine
                XDocument newDoc = new XDocument(new XElement("Logs"));
                newDoc.Save(xmlFilePath);
            }

            // Chargement du fichier XML
            XDocument doc = XDocument.Load(xmlFilePath);
            XElement logsElement = doc.Root;
            if (logsElement == null)
            {
                logsElement = new XElement("Logs");
                doc.Add(logsElement);
            }

            // Vérification de l'existence d'un élément avec le même nom
            bool exists = logsElement.Elements("Log")
                .Any(e => e.Element("Name")?.Value == name);

            if (!exists)
            {
                // Création d'un élément XML pour le log d'état
                XElement logElement = new XElement("Log",
                    new XElement("Name", name),
                    new XElement("SourceFilePath", sourcePath),
                    new XElement("TargetFilePath", targetPath),
                    new XElement("State", state.ToString()),
                    new XElement("TotalFilesToCopy", nbFileToCopy),
                    new XElement("TotalFilesSize", fileSize),
                    new XElement("NbFilesLeftToDo", nbFileLeftToDo),
                    new XElement("Progression", progression));

                // Ajout de l'élément XML au fichier
                logsElement.Add(logElement);
                doc.Save(xmlFilePath);
            }
            else
            {
                XElement existingLogElement = logsElement.Elements("Log")
                    .FirstOrDefault(e => e.Element("Name")?.Value == name);

                if (existingLogElement != null)
                {
                    existingLogElement.Element("SourceFilePath")?.SetValue(sourcePath);
                    existingLogElement.Element("TargetFilePath")?.SetValue(targetPath);
                    existingLogElement.Element("State")?.SetValue(state.ToString());
                    existingLogElement.Element("TotalFilesToCopy")?.SetValue(nbFileToCopy);
                    existingLogElement.Element("TotalFilesSize")?.SetValue(fileSize);
                    existingLogElement.Element("NbFilesLeftToDo")?.SetValue(nbFileLeftToDo);
                    existingLogElement.Element("Progression")?.SetValue(progression);

                    doc.Save(xmlFilePath);
                }
            }
        }


        public void DisplayLog()
        {
            string logContents = File.ReadAllText(_path);
            Console.WriteLine(logContents);
        }
    }
}

