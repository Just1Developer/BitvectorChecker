using System.Text;
using System.Text.RegularExpressions;

namespace BitvectorChecker;

public class Evaluator
{
    internal class Data
    { 
        internal long Length;
        internal double Fill;
        internal char Type;
        
        internal long Queries;
        internal long Space;
        internal double Overhead; 
        internal long Time;
        internal long QueryTotalTime;
        internal double QuerySingleAverage;
    }
    
    public static void EvaluateAll(string folder, string engine = "cs-tulip")
    {
        if (!Directory.Exists(folder))
        {
            Console.WriteLine($"Folder {folder} does not exist.");
            return;
        }

        Regex regex = new Regex(@"input0-f(\d+,\d*)-s(\d+)-q(\d+)-t([a|r|s]).in$");
        string[] files = Directory.GetFiles(folder);
        // Sort the files by creation time
        var sortedFiles = files.OrderBy(f => File.GetCreationTime(f)).ToArray();

        Dictionary<char, Dictionary<long, Dictionary<double, Data>>> data = new ();
        // Represents: <query type, <length, <fill, Data>>>

        foreach (string file in sortedFiles)
        {
            var match = regex.Match(file);
            if (!match.Success) continue;
            string fill = match.Groups[1].Value;
            long length = long.Parse(match.Groups[2].Value);
            long queries = long.Parse(match.Groups[3].Value);
            char type = match.Groups[4].Value[0];

            Data _data = RunOnlyEval(length, queries, fill, type, engine);
            if (_data == null) continue;

            if (!data.TryGetValue(type, out var _dictionary))
            {
                _dictionary = new Dictionary<long, Dictionary<double, Data>>();
                data[type] = _dictionary;
            }

            if (!_dictionary.TryGetValue(length, out var _daDictionary))
            {
                _daDictionary = new Dictionary<double, Data>();
                _dictionary[length] = _daDictionary;
            }

            _daDictionary[_data.Fill] = _data;
        }
        
        // We can just afford to iterate multiple times, and its easier and faster to code
        
        // Print in table format, where top is fill, left is size, one table per type
        StringBuilder fileBuilder = new StringBuilder();
        StringBuilder fileBuilder2 = new StringBuilder();
        StringBuilder fileBuilder3 = new StringBuilder();

        Dictionary<long, KeyValuePair<long, long>> overheads = new();
        foreach (char c in data.Keys)
        {
            Console.WriteLine($"Query Type: {c}");
            fileBuilder.AppendLine($"Query Type: {c}");
            fileBuilder2.AppendLine($"Total time - Query Type: {c}");
            var dict = data[c];
            foreach (long l in dict.Keys)
            {
                var dict2 = dict[l];
                long time = 0;
                int amt = 0;
                foreach (double d in dict2.Keys)
                {
                    var _data = dict2[d];
                    Console.Write($"{_data.QuerySingleAverage}\t");
                    fileBuilder.Append($"{_data.QuerySingleAverage}\t");
                    time += _data.Time;
                    ++amt;
                    if (!overheads.ContainsKey(_data.Length)) overheads.Add(_data.Length, new KeyValuePair<long, long>(0, 0));
                    overheads[_data.Length] = new KeyValuePair<long, long>(overheads[_data.Length].Key + _data.Space, overheads[_data.Length].Value + 1);
                }
                Console.WriteLine();
                fileBuilder.AppendLine();
                fileBuilder2.AppendLine($"{(long) Math.Round((double) time / amt)}");
            }
        }
        fileBuilder3.AppendLine($"Total Sizes:");
        foreach (var k in overheads.Keys)
        {
            var kvp = overheads[k];
            //double d = ((((double)kvp.Key / kvp.Value) * 100) / (100 * k));
            //fileBuilder3.AppendLine($"{k}\t{(long) Math.Round((double)kvp.Key / kvp.Value)}\t{d}");
            fileBuilder3.AppendLine($"{k}\t{(long) Math.Round((double)kvp.Key / kvp.Value)}");
        }

        fileBuilder.AppendLine(fileBuilder2.ToString()).AppendLine(fileBuilder3.ToString());
        
        File.WriteAllText("./eval.tsv", fileBuilder.ToString());
    }
    
    
    internal static Data RunOnlyEval(long length, long queryCount, string fill, char queryType, string engine)
    {
        string filename = $"input0-f{fill}-s{length}-q{queryCount}-t{queryType}";
        
        var cpp_log = Executable.PrimitiveRun(Executable.GetBitvectorProcess3_unix("./inputEval/" + filename + ".in", engineName: engine), null);

        if (cpp_log.Count == 0)
        {
            return null; //"--- no read for file " + filename;
        }

        if (cpp_log.Count > 0 && cpp_log[0] == "This is a test!")
        {
            cpp_log.RemoveRange(0, 2);
        }
        
        string resultEntry = cpp_log.Count == 0 ? "-" : cpp_log[cpp_log.Count - 1];
        string evalEntry = cpp_log.Count < 2 ? "-" : cpp_log[cpp_log.Count - 2];

        if (resultEntry.StartsWith("EVAL"))
        {
            resultEntry = evalEntry;
            evalEntry = cpp_log.Count == 0 ? "-" : cpp_log[cpp_log.Count - 1];
        }
        
        long Time, Space;
        
        if (resultEntry.StartsWith("RESULT"))
        {
            var regex = new Regex(@"time=(\d+) space=(\d+)");
            var match = regex.Match(resultEntry);
            if (match.Success)
            {
                Time = long.Parse(match.Groups[1].Value);
                Space = long.Parse(match.Groups[2].Value);
            }
            else
            {
                Time = 1;
                Space = 1;
            }
        }
        else
        {
            Time = 1;
            Space = 1;
        }

        long QueryTime = -1;
        if (evalEntry.StartsWith("EVAL"))
        {
            var regex = new Regex(@"time=(\d+)");
            var match = regex.Match(evalEntry);
            if (match.Success)
            {
                QueryTime = long.Parse(match.Groups[1].Value);
            }
        }

        double overheadExact = ((double)Space / length - 1) * 100;
        var TimePerQueryInNS = (double) QueryTime / queryCount;

        //return $"length={length} queries={queryCount} fill={fill} type={queryType}: space={Space} -overhead={overheadExact} time={Time} query-time={QueryTime} time-per-query={TimePerQueryInNS}";
        return new Data()
        {
            Length = length,
            Type = queryType,
            Fill = double.Parse(fill),
            
            Space = Space,
            Time = Time,
            Overhead = overheadExact,
            Queries = queryCount,
            QueryTotalTime = QueryTime,
            QuerySingleAverage = TimePerQueryInNS
        };
    }
}