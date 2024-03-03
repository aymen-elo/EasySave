using System.Diagnostics;

namespace EasySaveGUI.Helper
{
    public class CryptoSoftCipher
    {
        public string SendToCryptoSoft(string sourcePath, string targetPath, string encryptionKey)
        {
            string arguments = sourcePath + " " + targetPath + " " + encryptionKey;
            Process process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = "../../../../CryptoSoft/bin/Debug/net5.0/CryptoSoft.exe",
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                }
            };
            process.Start();
            while (!process.StandardOutput.EndOfStream)
            {
                string? line = process.StandardOutput.ReadLine();
                return line;
            }
            process.Close();
            return "0";
        }
    }
}