using System.Diagnostics;
using System.Text;
using CSharp_Kubernetes.Overwatch;

namespace BitvectorChecker;

public class Testcase
{
    private readonly string filepath;
    internal readonly Bitvector Bitvector;
    internal readonly List<string> Queries;
    
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
        for (int i = 0; i < int.Parse(file[0]); ++i)
        {
            Queries.Add(file[i + 2]);
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
    /// <param name="log">If success cases should be logged or not.</param>
    /// <returns>If all queries matches the expected results.</returns>
    public bool Run(bool log)
    {
        StringBuilder output = new StringBuilder();
        output.AppendLine("\n---------------------- Begin  Analysis ----------------------");
        output.AppendLine($"Output for input file {filepath}");
        output.AppendLine($"Bitvector: {Bitvector.ToString()} (length: {Bitvector.ToString().Length})");
        
        List<string> results_cs = new List<string>();
        foreach (string query in Queries)
        {
            results_cs.Add(Bitvector.ProcessCommand(query));
        }

        var cpp_log = Executable.PrimitiveRun(Executable.GetBitvectorProcess2(filepath), output);
        
        // For some reason, the executable isnt updated properly.
        if (cpp_log.Count > 0 && cpp_log[0] == "This is a test!")
        {
            cpp_log.RemoveRange(0, 2);
        }
        
        int max = Math.Min(results_cs.Count, cpp_log.Count);
        output.AppendLine($"Output comparisons: {max} (cs: {results_cs.Count}, cpp: {cpp_log.Count})");
        output.AppendLine("       Query        Expected (C#)        Returned (C++)");
        
        bool success = true;
        for (int i = 0; i < max; i++)
        {
            if (results_cs[i].Trim() == cpp_log[i].Trim())
            {
                if (!log) continue;
                output.AppendLine($"{Queries[i]}               {results_cs[i]}                   {cpp_log[i]}");
                continue;
            }

            success = false;
            output.AppendLine($"[FAIL] {Queries[i]}         {results_cs[i]}                   {cpp_log[i]}");
        }

        if (!log && success) output.AppendLine("\n                [ No Failures to Show ]\n");
        output.AppendLine("---------------------- End of Analysis ----------------------");
        Console.WriteLine(output.ToString());
        return success;
    }
}