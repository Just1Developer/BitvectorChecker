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

        int i = 0;
        foreach (string file in sortedFiles)
        {
            if (i++ == 5) break;
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
            
            /*
            Dictionary<long, Dictionary<double, Data>> _dictionary = 
                data.GetValueOrDefault(type, new Dictionary<long, Dictionary<double, Data>>());
            
            Dictionary<double, Data> _daDictionary = 
                _dictionary.GetValueOrDefault(length, new Dictionary<double, Data>());
            
            _daDictionary.Add(_data.Fill, _data);
            _dictionary.Add(length, _daDictionary);
            data.Add(type, _dictionary);
            */
        }
        
        // Print in table format, where top is fill, left is size, one table per type
        StringBuilder fileBuilder = new StringBuilder();
        foreach (char c in data.Keys)
        {
            Console.WriteLine($"Query Type: {c}");
            fileBuilder.AppendLine($"Query Type: {c}");
            var dict = data[c];
            foreach (long l in dict.Keys)
            {
                var dict2 = dict[l];
                foreach (double d in dict2.Keys)
                {
                    var _data = dict2[d];
                    Console.Write($"{_data.QuerySingleAverage}\t");
                    fileBuilder.Append($"{_data.QuerySingleAverage}\t");
                }
                Console.WriteLine();
                fileBuilder.AppendLine();
            }
        }
        
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
                Time = int.Parse(match.Groups[1].Value);
                Space = int.Parse(match.Groups[2].Value);
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