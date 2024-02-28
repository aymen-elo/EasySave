using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Windows.Controls;
using EasySave.Model;
using EasySaveLib.Model;
using EasySaveRemote;
using Newtonsoft.Json;
using Job = EasySaveLib.Model.Job;
using JobState = EasySaveLib.Model.JobState;

namespace EasySaveRemote.Packets
{

    public enum MessageType
    {
        GAJ,
        RJ,
        EJ,
        DJ,
        NJ,
        PP,
        SJ,
        MO
    }

    public class SendMessage
    {
        private MainViewModel _mainViewModel;
        private ConfigManager _configManager;
        private static FormatLog _formatLog = new FormatLog();
        
        private static ObservableCollection<Job> _jobs = new ObservableCollection<Job>();
        public static ObservableCollection<Job> Jobs => _jobs;
        public static event Action<ObservableCollection<Job>> JobsUpdated;
        
        private static void AddJob(Job job)
        {
            _jobs.Add(job);
            JobsUpdated?.Invoke(_jobs);
        }
        
        public static async void SendMessageTo(string ipAddress, int port, string message, MessageType messageType, MainWindow _mainWindow = null)
        {
            try
            {
                message = $"<|{messageType}|>" + message;
                bool oneTime = false;
                while (!oneTime)
                {
                    // Send message.
                    var messageBytes = Encoding.UTF8.GetBytes(message);
                    await _mainWindow.client.SendAsync(messageBytes, SocketFlags.None);


                    // Receive data. or ack
                    var buffer = new byte[4_096];
                    var received = await _mainWindow.client.ReceiveAsync(buffer, SocketFlags.None);
                    var response = Encoding.UTF8.GetString(buffer, 0, received);
                    switch (response)
                    {
                        case var str when str.StartsWith("<|GAJ|>"):
                            /* Get All Jobs */
                            var responseNoPrefix = response.Replace("<|GAJ|>", "");
                            var content = JsonConvert.DeserializeObject<ObservableCollection<Job>>(responseNoPrefix);
                            

                            foreach (var j in content)
                            {
                                if (j.State == JobState.Retired) { continue; }
                                var job = new Job(j.Name, j.BackupType, j.State, j.SourceFilePath, j.TargetFilePath, 
                                    j.TotalFilesToCopy, j.NbFilesLeftToDo, j.TotalFilesSize);
                                    AddJob(job);
                            }
                            
                            /* Send Ack to server */
                            await _mainWindow.client.SendAsync(new byte[1], SocketFlags.None);

                            
                            // ObservableCollection<Job> jobs = new ObservableCollection<Job>();
                            oneTime = true;
                            break;
                        case var str when str.StartsWith("<|RJ|>"):
                            /* Run Jobs */
                            oneTime = true;
                            break;
                        case var str when str.StartsWith("<|EJ|>"):
                            /* Edit Jobs */
                            oneTime = true;
                            break;
                        case var str when str.StartsWith("<|DJ|>"):
                            /* Delete Jobs */
                            oneTime = true;
                            break;
                        case var str when str.StartsWith("<|NJ|>"):
                            /* New Job */
                            oneTime = true;
                            break;
                        case var str when str.StartsWith("<|PP|>"):
                            /* Play/Pause a Job */
                            oneTime = true;
                            break;
                        case var str when str.StartsWith("<|Stop|>"):
                            /* Stop Running Jobs */
                            oneTime = true;
                            break;
                        case var str when str.StartsWith("<|MO|>"):
                            /* Modify Option */
                            oneTime = true;
                            break;
                        case var str when str.StartsWith("<|Opt|>"):
                            /* Received Option */

                            responseNoPrefix = response.Replace("<|Opt|>", "");
                            string[] dataArray = responseNoPrefix.Split(';');
                            
                            _mainWindow.language = dataArray[0];
                            _mainWindow.logFormat = dataArray[1];
                            _mainWindow.encryptionKey = dataArray[2];
                            _mainWindow.cipherList = dataArray[3];
                            _mainWindow.priorityList = dataArray[4];
                            _mainWindow.bigFileSize = dataArray[5];
                            
                            oneTime = true;
                            break;
                        default:
                            /* Type not recognized */
                            oneTime = false;
                            break;
                    }
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}