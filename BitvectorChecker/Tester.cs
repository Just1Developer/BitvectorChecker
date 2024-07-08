using System.Text;

namespace BitvectorChecker;

public class Tester
{
    private static Random Random = new ();
    
    public static Testcase NewTest(int minLength, int maxLength) => NewTest(Random.Next(minLength, maxLength)); 
    public static Testcase NewTest(int length)
    {
        StringBuilder vectorbuilder = new StringBuilder();
        StringBuilder commandBuilder = new StringBuilder();
        commandBuilder.AppendLine("std::vector<string> filecontents = {");
        List<string> commands = new List<string>();
        int ones = 0, zeros = 0;
        for (int i = 0; i < length; ++i)
        {
            if (Random.NextDouble() > 0.5)
            {
                vectorbuilder.Append("1");
                ones++;
            }
            else
            {
                vectorbuilder.Append("0");
                zeros++;
            }
            AddCommandToBuilder(ref commandBuilder, ref commands, i);
        }

        for (int i = 1; i <= zeros; ++i)
        {
            commandBuilder.AppendLine($"select 0 {i},");
            commands.Add($"select 0 {i}");
        }
        for (int i = 1; i <= ones; ++i)
        {
            commandBuilder.AppendLine($"select 1 {i},");
            commands.Add($"select 1 {i}");
        }
        
        commandBuilder.Insert(0, $"{vectorbuilder}\n");
        commandBuilder.Insert(0, "std::vector<string> filecontents_2 = {\n");
        commandBuilder.AppendLine("};");

        Console.WriteLine("Bitvector: " + vectorbuilder);
        
        // Generate Testcase and Input file
        int j = 0;
        string path = "./input/input";
        if (!Directory.Exists("./input/")) Directory.CreateDirectory("./input/");
        while (File.Exists(path + $"{j}.in")) j++;

        string _path = $"{path}{j}.in";

        StringBuilder fileContentBuilder = new StringBuilder();
        fileContentBuilder.AppendLine(commands.Count + "");
        fileContentBuilder.AppendLine(vectorbuilder.ToString());
        foreach (string cmd in commands) fileContentBuilder.AppendLine(cmd);
        
        if (!Directory.Exists("./input/"))
            Directory.CreateDirectory("./input/");
        File.WriteAllText(_path, fileContentBuilder.ToString());
        Testcase testcase = new Testcase(_path);
        return testcase;
    }
    
    public static void NewSparseTestFile(int minLength, int maxLength) => NewSparseTestFile(Random.Next(minLength, maxLength)); 
    public static void NewSparseTestFile(int length)
    {
        StringBuilder vectorbuilder = new StringBuilder();
        List<string> commands = new List<string>();
        int ones = 0, zeros = 0;
        for (int i = 0; i < length; ++i)
        {
            if (Random.NextDouble() > 0.5)
            {
                vectorbuilder.Append("1");
                ones++;
            }
            else
            {
                vectorbuilder.Append("0");
                zeros++;
            }
        }
        
        // add sparse queries
        // Check 50% of edges for rank:
        if (ones > 0)
        {
            commands.Add("select 1 1");
            commands.Add($"select 1 {ones}");
        }

        if (zeros > 0)
        {
            commands.Add($"select 0 1");
            commands.Add($"select 0 {zeros}");
        }

        for (int i = 4095; i < length; i += 4096)
        {
            if (Random.NextSingle() > 0.5f) continue;
            commands.Add($"rank 0 {i}");
            commands.Add($"rank 1 {i}");
            commands.Add($"rank 0 {i+1}");
            commands.Add($"rank 1 {i+1}");
        }
        
        // Random access queries:
        for (int k = 0; k < 1000; ++k)
        {
            float rnd = Random.NextSingle();
            if (rnd < 0.1) commands.Add($"access {Random.Next(0, length)}");
            else if (rnd < 0.4)
            {
                commands.Add($"rank 0 {Random.Next(0, length)}");
                commands.Add($"rank 1 {Random.Next(0, length)}");
            }
            else
            {
                commands.Add($"select 0 {Random.Next(0, ones) + 1}");
                commands.Add($"select 1 {Random.Next(0, zeros) + 1}");
            }
        }

        Console.WriteLine("Created Bitvector of Length: " + length);
        
        // Generate Testcase and Input file
        int j = 0;
        string path = "./input/input";
        if (!Directory.Exists("./input/")) Directory.CreateDirectory("./input/");
        while (File.Exists(path + $"{j}.in")) j++;

        string _path = $"{path}{j}.in";

        StringBuilder fileContentBuilder = new StringBuilder();
        fileContentBuilder.AppendLine(commands.Count + "");
        fileContentBuilder.AppendLine(vectorbuilder.ToString());
        foreach (string cmd in commands) fileContentBuilder.AppendLine(cmd);
        
        if (!Directory.Exists("./input/"))
            Directory.CreateDirectory("./input/");
        File.WriteAllText(_path, fileContentBuilder.ToString());
    }

    public static void AddCommandToBuilder(ref StringBuilder builder, ref List<string> list, int index)
    {
        string[] cmds = {
            "access " + index,
            "rank 0 " + index,
            "rank 1 " + index,
        };
        foreach (string cmd in cmds)
        {
            builder.AppendLine(cmd + ",");
            list.Add(cmd);
        }
    }
}