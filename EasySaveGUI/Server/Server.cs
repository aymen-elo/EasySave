using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Media;
using EasySaveGUI.View;
using EasySaveLib.Model;

namespace EasySaveGUI.Server
{
    /// <summary>
    /// Server class for handling incoming network requests.
    /// </summary>
    public class Server
    {
        private readonly MainWindow _mainWindow;

        /// <summary>
        /// Constructor for the Server class.
        /// </summary>
        /// <param name="mainWindow">The main window of the application.</param>
        public Server(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
        }

        /// <summary>
        /// Starts the server to listen for incoming requests.
        /// </summary>
        public async void Serve()
        {
            var executeCommand = new ExecuteCommand();
            try
            {
                IPEndPoint ipEndPoint = new(IPAddress.Parse("127.0.0.1"), 13);
                using Socket listener = new(
                    ipEndPoint.AddressFamily,
                    SocketType.Stream,
                    ProtocolType.Tcp);

                var prefixes = new List<string>
                    { "<|GAJ|>", "<|RJ|>", "<|EJ|>", "<|DJ|>", "<|NJ|>", "<|PP|>", "<|SJ|>", "<|MO|>" };


                listener.Bind(ipEndPoint);
                listener.Listen(100);

                var handler = await listener.AcceptAsync();
                string responseNoPrefix;

                while (true)
                {
                    var buffer = new byte[4_096];
                    // Receive message.
                    var received = await handler.ReceiveAsync(buffer, SocketFlags.None);
                    while (received == 0) received = await handler.ReceiveAsync(buffer, SocketFlags.None);

                    var response = Encoding.UTF8.GetString(buffer, 0, received);

                    var eom = prefixes.FirstOrDefault(prefix => response.StartsWith(prefix));
                    switch (eom)
                    {
                        case var str when str == "<|GAJ|>":
                            /* Get All Jobs */
                            var statePath = @"C:\Prosoft\EasySave\Logs\state.json";
                            var stateContent = File.ReadAllText(statePath);
                            stateContent = "<|GAJ|>" + stateContent;

                            var echoBytes = Encoding.UTF8.GetBytes(stateContent);
                            await handler.SendAsync(echoBytes, 0);

                            /* Wait until client ack */
                            await handler.ReceiveAsync(buffer, SocketFlags.None);
                            var conf =
                                $"{ConfigManager.GetSavedLanguage()};{ConfigManager.GetLogFormat()};{ConfigManager.GetEncryptionKey()};{ConfigManager.GetCipherList()};{ConfigManager.GetPriorityList()};{ConfigManager.GetBigFileSize()}";
                            conf = "<|Opt|>" + conf;
                            echoBytes = Encoding.UTF8.GetBytes(conf);
                            await handler.SendAsync(echoBytes, 0);

                            break;

                        case var str when str == "<|RJ|>":
                            /* Run Jobs */
                            responseNoPrefix = response.Replace("<|RJ|>", "");
                            executeCommand.RJExecute(responseNoPrefix);
                            break;


                        case var str when str == "<|EJ|>":
                            /* Edit Jobs */
                            responseNoPrefix = response.Replace("<|EJ|>", "");
                            executeCommand.EJExecute(responseNoPrefix);
                            break;


                        case var str when str == "<|DJ|>":
                            /* Delete Jobs */
                            responseNoPrefix = response.Replace("<|DJ|>", "");
                            _mainWindow.JobsViewModel.DeleteJob(responseNoPrefix);
                            break;

                        case var str when str == "<|NJ|>":
                            /* New Job */
                            responseNoPrefix = response.Replace("<|NJ|>", "");
                            executeCommand.NJExecute(responseNoPrefix);
                            break;


                        case var str when str == "<|PP|>":
                            /* Play/Pause a Job */
                            if (!MainWindow.IsPaused)
                                MainWindow.IsPaused = true;
                            else
                                MainWindow.IsPaused = false;
                            break;
                        case var str when str == "<|SJ|>":
                            /* Stop Running Jobs */
                            if (!MainWindow.IsStopped)
                            {
                                MainWindow.IsStopped = true;
                                _mainWindow.btnStopJob.Background = Brushes.Red;
                            }
                            else
                            {
                                MainWindow.IsStopped = false;
                                _mainWindow.btnStopJob.Background = _mainWindow.BackgroundColor;
                            }

                            break;
                        case var str when str == "<|MO|>":
                            /* Modify Option */
                            responseNoPrefix = response.Replace("<|MO|>", "");
                            executeCommand.MOExecute(responseNoPrefix);
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