using System.Collections.Generic;
using System.Diagnostics;

namespace EasySaveGUI.Model
{
    public class ProcessBL
    {
        public static bool IsDetected { get; set; }
        
        public static bool IsProcessRunning(List<string> listProcess)
        {
            foreach (string processName in listProcess)
            {
                Process[] processes = Process.GetProcessesByName(processName);
                if (processes.Length > 0)
                {
                    return IsDetected = true;
                }
            }

            return IsDetected = false;
        }
    }
}