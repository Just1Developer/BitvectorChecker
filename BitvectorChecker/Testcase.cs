using System.Text;
using System.Text.RegularExpressions;

namespace BitvectorChecker;

public class Testcase
{
    public int Time { get; private set; }
    public int Space { get; private set; }
    
    public int TotalTests { get; private set; }
    public int FailedTests { get; private set; }
    
    public int QueryCount { get => Queries.Count; }
    
    private readonly string filepath;
    internal readonly Bitvector Bitvector;
    private readonly List<string> Queries;
    List<string> ResultComparator;
    
    /// <summary>
    /// Creates a new test case by reading the bitvector and queries from the file like the c++ program
    /// as specified in the project instructions.
    /// Creates a new Bitvector from the files second line, and processes a number of commands after, the
    /// amount of commands is specified in line 1.
    /// </summary>
    /// <param name="filepath">The path of the input file. Will be relayed to the c++ program later as well.</param>
    public Testcase(string filepath)
    {
        this.filepath = filepath;
        string[] file = File.ReadAllLines(filepath);
        this.Bitvector = new Bitvector(file[1]);
        this.Queries = new List<string>();
        /*
        for (int i = 0; i < int.Parse(file[0]) && i < 1000; ++i)
        {
            Queries.Add(file[i + 2]);
        } 
        */
        for (int i = 0; i < 100; ++i)
        {
            Queries.Add($"rank 0 {i}");
            Console.WriteLine($"rank 0 {i}");
        }
        
        ResultComparator = new List<string>();
        foreach (string query in Queries)
        {
            if (query == "") continue;
            Console.WriteLine("Processing Query " + query);
            ResultComparator.Add(Bitvector.ProcessCommand(query));
        }
    }

    /// <summary>
    /// 
    /// Runs the test. Creates a new Bitvector object and gets the query results.
    /// Then, runs the cpp executable bitvector with a file containing the same test vector and queries.
    /// Lastly, compares the results. Failed queries are printed as block, if <param name="log"></param> is set to true, all
    /// test results are added to this block. Failures will still be marked clearly.
    /// <p/>
    /// Returns if the test was successful, so if all query results from C++ matched with the C# Test-Bitvector.
    /// </summary>
    /// <param name="engine">The C++ executable file name.</param>
    /// <param name="log">If success cases should be logged or not.</param>
    /// <param name="testNumber">The test number. Must be positive or 0 to show up.</param>
    /// <returns>If all queries matches the expected results.</returns>
    public bool Run(bool log, string engine = "bitvector", int testNumber = -1)
    {
        // Reset Values

        TotalTests = 0;
        FailedTests = 0;
        long size = Bitvector.ToString().Length;
        
        // Test
        
        StringBuilder output = new StringBuilder();
        output.AppendLine($"\n---------------------- {(testNumber >= 0 ? $"Test {testNumber} | " : "")}Begin  Analysis ----------------------");
        output.AppendLine($"Output for input file {filepath}");
        output.AppendLine($"Engine: {engine}");
        output.AppendLine($"Bitvector: {(size > 10000 ? "<big vector>" : Bitvector.ToString())} (length: {size})");

        var cpp_log = Executable.PrimitiveRun(Executable.GetBitvectorProcess2(filepath, engine), output);

        if (cpp_log == null || cpp_log.Count == 0)
        {
            cpp_log = new List<string>();
            for (int i = 0; i < 100; ++i)
            {
                cpp_log.Add("-1");
            }
        }

        // For some reason, the executable isnt updated properly.
        if (cpp_log.Count > 0 && cpp_log[0] == "This is a test!")
        {
            cpp_log.RemoveRange(0, 2);
        }
        
        int max = Math.Min(ResultComparator.Count, cpp_log.Count);
        output.AppendLine($"Output comparisons: {max} (cs: {ResultComparator.Count}, cpp: {cpp_log.Count})");
        output.AppendLine("   Query                Expected (C#)            Returned (C++)");
        
        bool success = true;
        for (int i = 0; i < max; i++)
        {
            if (ResultComparator[i].Trim() == cpp_log[i].Trim())
            {
                if (!log) continue;
                output.AppendLine(Format(Queries[i], ResultComparator[i], cpp_log[i]));
                continue;
            }

            success = false;
            output.AppendLine(Format($"[FAIL] {Queries[i]}", ResultComparator[i], cpp_log[i]));
            FailedTests++;
        }
        TotalTests = max;

        if (!log && success) output.AppendLine("\n                        [ No Failures to Show ]\n");

        string resultEntry = cpp_log.Count == 0 ? "-" : cpp_log[cpp_log.Count - 1];
        if (resultEntry.StartsWith("RESULT"))
        {
            var regex = new Regex(@"time=(\d+) space=(\d+)");
            var match = regex.Match(resultEntry);
            if (match.Success)
            {
                Time = int.Parse(match.Groups[1].Value);
                Space = int.Parse(match.Groups[2].Value);
            }
            else
            {
                Time = 1;
                Space = 1;
            }
        }

        double overheadExact = ((double)Space / size - 1) * 100;
        output.AppendLine("---------------------- =============== ----------------------");
        output.AppendLine($"Overhead (Round): {Math.Round(overheadExact, 5)}%");
        output.AppendLine($"Overhead (Exact): {overheadExact}%");
        output.AppendLine("---------------------- End of Analysis ----------------------");
        Console.WriteLine(output.ToString());
        
        return success;
    }

    private const int QueryStringMaxLength = 25;
    private const int ResultNumberLength = 15;

    private string Format(string query, string expected, string result)
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(query);
        for (int i = query.Length; i <= QueryStringMaxLength; ++i) builder.Append(' ');
        
        for (int i = expected.Length; i <= ResultNumberLength; ++i) builder.Append(' ');
        builder.Append(expected);

        builder.Append("               ");
        for (int i = result.Length; i <= ResultNumberLength; ++i) builder.Append(' ');
        builder.Append(result);

        return builder.ToString();
    }
}