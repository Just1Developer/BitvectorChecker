using System.Diagnostics;
using System.Text;

namespace BitvectorChecker;

public static class Executable
{
    public const string program_name = "bitvector2";
    
    internal static Process GetBitvectorProcess2(string inputFileName)
    {
        string executablePath = $"./{program_name}";  // Path to the executable in the application directory
        string arguments = inputFileName;

        Process process = new Process();
        process.StartInfo = GetStartInfo(Environment.CurrentDirectory, executablePath, arguments);
        process.EnableRaisingEvents = true;

        return process;
    }

    internal static List<string> PrimitiveRun(Process process, StringBuilder? log)
    {
        process.Start();

        List<string> outputLines = new List<string>();
        while (!process.StandardOutput.EndOfStream)
        {
            string? line = process.StandardOutput.ReadLine();
            if (!string.IsNullOrEmpty(line))
            {
                if (line.StartsWith("RESULT"))
                {
                    if (log == null) Console.WriteLine(line);
                    else log.AppendLine(line);
                }
                outputLines.Add(line);
            }
        }

        process.WaitForExit();
        return outputLines;
    }

    internal static ProcessStartInfo GetStartInfo(string workingDirectory, string processName, string arguments)
    {
        return new ProcessStartInfo()
        {
            WorkingDirectory = workingDirectory,
            FileName = processName,
            Arguments = arguments,
            CreateNoWindow = true, // This prevents the command window from showing up
            UseShellExecute = false, // Necessary to redirect input/output if needed
            RedirectStandardOutput = true, // To capture the output
            RedirectStandardError = true // To capture errors
        };
    }
}