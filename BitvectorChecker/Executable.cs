using System.Diagnostics;
using System.Text;

namespace BitvectorChecker;

public static class Executable
{
    public const string program_name = "bitvector";

    internal static Process GetBitvectorProcess2(string inputFileName, string engineName = program_name)
    {
        if (Environment.OSVersion.Platform.ToString().StartsWith("Win"))
        {
            return GetBitvectorProcess2_win(inputFileName, engineName);
        }
        return GetBitvectorProcess2_unix(inputFileName, engineName);
    }
    private static Process GetBitvectorProcess2_unix(string inputFileName, string engineName = program_name)
    {
        if (!File.Exists(engineName))
        {
            Console.Error.WriteLine($"Engine {engineName} does not exist, switching to default {program_name}");
            engineName = program_name;
        }
        string executablePath = $"./{engineName}";  // Path to the executable in the application directory
        string arguments = inputFileName;

        Process process = new Process();
        process.StartInfo = GetStartInfo(Environment.CurrentDirectory, executablePath, arguments);
        process.EnableRaisingEvents = true;

        return process;
    }
    internal static Process GetBitvectorProcess3_unix(string inputFileName, string outputFileName = "./out/output.txt", string engineName = program_name)
    {
        if (!File.Exists(engineName))
        {
            Console.Error.WriteLine($"Engine {engineName} does not exist, switching to default {program_name}");
            engineName = program_name;
        }
        string executablePath = $"./{engineName}";  // Path to the executable in the application directory
        string arguments = $"{inputFileName} {outputFileName}";

        Process process = new Process();
        process.StartInfo = GetStartInfo(Environment.CurrentDirectory, executablePath, arguments);
        process.EnableRaisingEvents = true;

        return process;
    }
    private static Process GetBitvectorProcess2_win(string inputFileName, string engineName)
    {
        engineName += ".exe";
        if (!File.Exists(engineName))
        {
            Console.Error.WriteLine($"Engine {engineName} does not exist, switching to default {program_name}");
            engineName = program_name;
        }
        string executablePath = Path.Combine(Environment.CurrentDirectory, engineName);  // Path to the executable in the application directory
        string arguments = inputFileName;

        Process process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = executablePath,
            Arguments = arguments,
            WorkingDirectory = Environment.CurrentDirectory,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
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
        Console.WriteLine($"CPP Process finished with exit code {process.ExitCode}.");
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