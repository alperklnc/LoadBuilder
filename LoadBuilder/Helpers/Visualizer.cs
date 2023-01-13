using System;
using System.Diagnostics;

namespace LoadBuilder.Helpers
{
    public static class Visualizer
    {
        public static void VisualizeOutput(string path, string fileName)
        {
            Process p = new Process();
            p.StartInfo.FileName = "python3";
            p.StartInfo.Arguments = $"visualizer.py {path}/Output {fileName}";

            p.StartInfo.RedirectStandardInput = true;
            
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.CreateNoWindow = true;

            p.StartInfo.WorkingDirectory = $"{path}/Helpers";
            p.StartInfo.UseShellExecute = false;

            p.Start();
            Console.WriteLine($"Total time to visualize data: {Program.Stopwatch.ElapsedMilliseconds} ms");
            //Console.WriteLine("Process StandardOutput");
            Console.Write(p.StandardOutput.ReadToEnd());
            //Console.WriteLine("Process StandardError");
            Console.Write(p.StandardError.ReadToEnd());
            p.WaitForExit();
            p.Close();
        }
    }
}