using System.Text;

namespace BitvectorChecker;

public class Tester
{
    private const uint mask = 0x7FFFFFFF;
    public static long RandomLong(long minLength, long maxLength)
    {
        return (((long)Random.Next(Math.Min((int)((minLength >> 31) & mask), (int)((maxLength >> 31) & mask)), Math.Max((int)((minLength >> 31) & mask), (int)((maxLength >> 31) & mask)))) << 31) |
               ((long)Random.Next((int)(minLength & mask), (int)(maxLength & mask)));
    }
    
    private static Random Random = new ();
    
    public static Testcase NewTest(int minLength, int maxLength, bool UseSparseCaching = false) => NewTest(Random.Next(minLength, maxLength), UseSparseCaching); 
    public static Testcase NewTest(int length, bool UseSparseCaching = false)
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
        Testcase testcase = new Testcase(_path, UseSparseCaching);
        return testcase;
    }
    
    public static String NewSparseTestFile(long minLength, long maxLength, string? randomParameter = null, long randomQueryCount = 1000) => NewSparseTestFile(
        RandomLong(minLength, maxLength), randomQueryCount); 
    public static String NewSparseTestFile(long length, long randomQueryCount = 1000)
    {
        StringBuilder vectorbuilder = new StringBuilder();
        List<string> commands = new List<string>();
        long ones = 0, zeros = 0;
        for (long i = 0; i < length; ++i)
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
        
        // Random access queries:
        for (int k = 0; k < randomQueryCount; ++k)
        {
            float rnd = Random.NextSingle();
            if (rnd < 0.1) commands.Add($"access {RandomLong(0, length)}");
            else if (rnd < 0.4)
            {
                commands.Add($"rank 0 {RandomLong(0, length)}");
                commands.Add($"rank 1 {RandomLong(0, length)}");
            }
            else
            {
                commands.Add($"select 0 {RandomLong(0, zeros) + 1}");
                commands.Add($"select 1 {RandomLong(0, ones) + 1}");
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

        return $"input{j}";
    }
    
    public static void NewSparseSpecificTestFile(long length, long queryCount, double fill, char queryType)
    {
        
        
        // Generate Testcase and Input file
        int j = 0;
        string path = "./inputEval/input";
        if (!Directory.Exists("./inputEval/")) Directory.CreateDirectory("./inputEval/");
        while (File.Exists(path + $"{j}.in")) j++;

        string _path = $"{path}{j}-f{fill}-s{length}-q{queryCount}-t{queryType}.in";

        if (!Directory.Exists("./inputEval/"))
            Directory.CreateDirectory("./inputEval/");

        using (var stream = new FileStream(_path, FileMode.OpenOrCreate, FileAccess.Write))
        using (var writer = new StreamWriter(stream, Encoding.ASCII))
        {
            writer.WriteLine(queryCount);

            long ones = 0, zeros = 0;
            for (long i = 0; i < length; ++i)
            {
                if (Random.NextDouble() < fill)
                {
                    writer.Write("1");
                    ones++;
                }
                else
                {
                    writer.Write("0");
                    zeros++;
                }
            }
            writer.WriteLine(); // To add a new line after bit vector
            
            
            if (queryType == 'a')
            {
                for (int k = 0; k < queryCount; ++k)
                {
                    writer.WriteLine($"access {RandomLong(0, length)}");
                }
            } else if (queryType == 'r')
            {
                for (int k = 0; k < queryCount; ++k)
                {
                    double rnd = Random.NextDouble();
                    if (rnd >= fill) {
                        writer.WriteLine($"rank 1 {RandomLong(0, length)}");
                    } else {
                        writer.WriteLine($"rank 0 {RandomLong(0, length)}");
                    }
                }
            } else if (queryType == 's')
            {
                for (int k = 0; k < queryCount; ++k)
                {
                    double rnd = Random.NextDouble();
                    if (rnd >= fill) {
                        writer.WriteLine($"select 1 {RandomLong(0, ones)}");
                    } else {
                        writer.WriteLine($"select 0 {RandomLong(0, zeros)}");
                    }
                }
            }
        }

        Console.WriteLine("Created Bitvector of Length: " + length);

        //return $"input{j}-f{fill}-s{length}-q{queryCount}-t{queryType}";
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