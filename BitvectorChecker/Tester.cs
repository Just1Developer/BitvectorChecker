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