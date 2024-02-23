using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        try
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();


            string sourceFile = args[0];        //  [0] = source_file
            string fileName = Path.GetFileName(sourceFile);
            string targetFile = args[1];        //  [1] = target_file
            string encryptionKey = args[2];     //  [2] = encryption_key
            int timeElapsed;
            
            if (Path.GetExtension(fileName) == ".cry")
            {
                targetFile = targetFile + "\\" + Path.GetFileNameWithoutExtension(fileName);       //  [1] = target_directory for decrypting
                timeElapsed = EncryptFile(sourceFile, targetFile, encryptionKey, stopwatch);
            }
            else
            {
                targetFile = targetFile + "\\" + fileName + ".cry";       //  [1] = target_directory for encrypting
                timeElapsed = EncryptFile(sourceFile, targetFile, encryptionKey, stopwatch);
            }
            Console.WriteLine(timeElapsed);
        }
        catch (Exception e)
        {
            Console.WriteLine(1);
        }
    }
    
    static int EncryptFile(string inputFile, string outputFile, string key, Stopwatch stopwatch)
    {
        using (FileStream inputStream = new FileStream(inputFile, FileMode.Open, FileAccess.Read))
        using (FileStream outputStream = new FileStream(outputFile, FileMode.Create, FileAccess.Write))
        using (StreamWriter writer = new StreamWriter(outputStream))
        {
            int keyIndex = 0;

            while (inputStream.Position < inputStream.Length)
            {
                int byteValue = inputStream.ReadByte();
                int keyValue = key[keyIndex];

                int encryptedValue = byteValue ^ keyValue;

                writer.Write(Convert.ToChar(encryptedValue));

                keyIndex = (keyIndex + 1) % key.Length;
            }
        }
        stopwatch.Stop();
        return stopwatch.Elapsed.Milliseconds;
    }
}