using System.Diagnostics;
using System.Text;

namespace CSharp_Kubernetes.Overwatch;

public static class Executable
{
    private const bool IS_DEBUG_MODE = false;
    
    private const string run_process_win = "cmd.exe";
    private const string run_process_unix = "/bin/bash";

    public const string program_name = "bitvector";

    // Windows versions are 0,1,2,3; 4 is unix, 5 is XBox, 6 is MacOS X, 7 is other
    internal static readonly bool IS_WINDOWS = (int) Environment.OSVersion.Platform < 4;
    private static readonly string run_process_cmd = IS_WINDOWS ? run_process_win : run_process_unix;

    public static void PrintInfo()
    {
        Console.WriteLine($"OS Version: {Environment.OSVersion.Platform} ({(int) Environment.OSVersion.Platform})");
        Console.WriteLine($"Is Windows: {IS_WINDOWS}");
        Console.WriteLine($"run_process_cmd: {run_process_cmd}");
    }
    
    private const string relativePathProd = @"/home/dev/Desktop/Overwatch/Streamy/StreamingService/";
    private const string absolutePathMacDebug = "../../../../../../WebstormProjects/StreamingService/";
    private const string absolutePathWinDebug = absolutePathMacDebug; // Todo
    private static readonly string WebServerRelativePath = Debugger.IsAttached || IS_DEBUG_MODE ?
        (IS_WINDOWS ? absolutePathWinDebug : absolutePathMacDebug) : relativePathProd;

    internal static string GetRelativeRepoPath() => WebServerRelativePath;
    
    /// <summary>
    /// Gets a new Process with the given start info. The process has raising events enabled by default.
    /// </summary>
    /// <param name="startInfo">The start info</param>
    /// <returns>The new process.</returns>
    internal static Process GetProcess(ProcessStartInfo startInfo)
    {
        Process process = new Process();
        process.StartInfo = startInfo;
        process.EnableRaisingEvents = true;
        return process;
    }
    
    /// <summary>
    /// Gets a new Process with the given info. The process has raising events enabled by default.
    /// </summary>
    /// <returns>The new process.</returns>
    internal static Process GetBitvectorProcess(string inputFileName)
    {
        Process process = new Process();
        process.StartInfo = GetStartInfo(".", "C++", $"-c \"./{program_name}\" {inputFileName}");
        process.EnableRaisingEvents = true;
        return process;
    }
    
    internal static Process GetBitvectorProcess2(string inputFileName)
    {
        string executablePath = $"./{program_name}";  // Path to the executable in the application directory
        string arguments = inputFileName;

        Process process = new Process();
        process.StartInfo = GetStartInfo(Environment.CurrentDirectory, executablePath, arguments);
        process.EnableRaisingEvents = true;

        return process;
    }

    internal static async Task<List<string>> RunProcessAsync(Process process, string processName, bool logOutput)
    {
        List<string> output = new List<string>();
        var tcs = new TaskCompletionSource<bool>();

        try
        {
            process.Exited += (sender, args) =>
            {
                tcs.SetResult(true);  // Set the result when the process exits
                process.Dispose();  // Dispose the process object
            };

            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    if(logOutput || e.Data.StartsWith("RESULT")) Console.WriteLine($">> [{processName}]: {e.Data}");
                    output.Add(e.Data);
                }
            };
            
            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    Console.WriteLine($">> [{processName} ERROR]: {e.Data}");
                    output.Add(e.Data);
                }
            };

            process.Start();
            process.BeginOutputReadLine();  // Begin asynchronous output reading
            Console.WriteLine($"Starting process {processName} " + process.StartInfo.WorkingDirectory + process.StartInfo.FileName);
            
            await tcs.Task;
            Console.WriteLine($"Process {processName} completed with exit code: ?" /*+ process.ExitCode*/);
        }
        catch (Exception e)
        {
            Console.WriteLine("An error occurred: " + e.Message);
        }

        return output;
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