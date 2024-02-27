using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Windows.Controls;
using EasySave.Model;
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
        Stop,
        MO
    }

    public class SendMessage
    {
        private MainViewModel _mainViewModel;
        
        private static ObservableCollection<Job> _jobs = new ObservableCollection<Job>();

        public ObservableCollection<Job> Jobs => _jobs;

        public static async void SendMessageTo(string ipAddress, int port, string message, MessageType messageType)
        {
            try
            {
                IPEndPoint ipEndPoint = new(IPAddress.Parse(ipAddress), port);
                using Socket client = new(
                    ipEndPoint.AddressFamily,
                    SocketType.Stream,
                    ProtocolType.Tcp);

                await client.ConnectAsync(ipEndPoint);
                message = $"<|{messageType}|>" + message;
                bool oneTime = false;


                while (!oneTime)
                {
                    // Send message.
                    var messageBytes = Encoding.UTF8.GetBytes(message);
                    _ = await client.SendAsync(messageBytes, SocketFlags.None);


                    // Receive data. or ack
                    var buffer = new byte[4_096];
                    var received = await client.ReceiveAsync(buffer, SocketFlags.None);
                    var response = Encoding.UTF8.GetString(buffer, 0, received);
                    switch (response)
                    {
                        case var str when str.StartsWith("<|GAJ|>"):
                            /* Get All Jobs */
                            Console.WriteLine($"Socket client received acknowledgment: \"{response}\"");
                            var responseNoPrefix = response.Replace("<|GAJ|>", "");
                            var content = JsonConvert.DeserializeObject<ObservableCollection<Job>>(responseNoPrefix);
                            

                            foreach (var j in content)
                            {
                                if (j.State == (JobState)JobState.Retired) { continue; }
                                var job = new Job(j.Name, j.BackupType, j.State, j.SourceFilePath, j.TargetFilePath, 
                                    j.TotalFilesToCopy, j.NbFilesLeftToDo, j.TotalFilesSize);
                                
                                _jobs.Add(job);
                            }
                            
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
                        default:
                            /* Type not recognized */
                            oneTime = false;
                            break;
                    }
                }

                client.Shutdown(SocketShutdown.Both);
            }
            catch (SocketException e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}