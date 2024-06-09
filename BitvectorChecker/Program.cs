// See https://aka.ms/new-console-template for more information

using System.Text;
using BitvectorChecker;

//TestMultiple(200, 10, 60);
var benchmarks = BenchmarkMultipleEngines(
	10,
	10,
	new[] { 20, 30 },
	new List<string>() { "bitvector", "bitvector2" }
	);
Console.WriteLine("\n\n\n----------------------------- Benchmark Results -----------------------------\n\n\n");
foreach (var benchmark in benchmarks) Console.WriteLine(benchmark.ToString());
Console.WriteLine("\n\n\n-------------------------- End of Benchmark Results --------------------------");

// ------------- Methods -------------

static bool TestSingleFile(int inputFileName, bool logAll)
{
	return new Testcase($"./input/{inputFileName}.in").Run(logAll);
}

static void TestMultiple(int amount, int min = 15, int max = 60, string engine = "bitvector")
{
	List<Testcase> testcases = new List<Testcase>();
	for (int i = 0; i < amount; ++i)
	{
		testcases.Add(Tester.NewTest(min, max));
	}

	foreach (Testcase testcase in testcases)
	{
		testcase.Run(false, engine);
	}
}

static List<BenchmarkResult> BenchmarkMultiple(int amount, int min, int[] maxima, string engine)
{
	List<BenchmarkResult> benchmarkResults = new List<BenchmarkResult>();
	int[] segmentBorders;
	if (maxima.Length == 0) segmentBorders = new int[] { min, min + 1 };
	else
	{
		segmentBorders = new int[maxima.Length + 1];
		maxima.CopyTo(segmentBorders, 1);
		segmentBorders[0] = min;
	}
	
	List<Testcase> testcases = new List<Testcase>();
	List<Testcase> allTestcases = new List<Testcase>();
	for (int i = 1; i < segmentBorders.Length; ++i)
	{
		testcases.Clear();
		for (int j = 0; j < amount; ++j)
		{
			var test = Tester.NewTest(segmentBorders[i - 1], segmentBorders[i]);
			testcases.Add(test);
			allTestcases.Add(test);
		}
		foreach (Testcase testcase in testcases)
		{
			testcase.Run(false, engine);
		}

		benchmarkResults.Add(new BenchmarkResult(testcases, segmentBorders[i - 1], segmentBorders[i], engine));
	}
	benchmarkResults.Insert(0, new BenchmarkResult(allTestcases, min, maxima[-1], engine));
	return benchmarkResults;
}

static List<List<BenchmarkResult>> BenchmarkMultipleEngines(int amount, int min, int[] maxima, List<string> engines)
{
	List<List<BenchmarkResult>> benchmarkResults = new List<List<BenchmarkResult>>();
	int[] segmentBorders;
	if (maxima.Length == 0) segmentBorders = new int[] { min, min + 1 };
	else
	{
		segmentBorders = new int[maxima.Length + 1];
		maxima.CopyTo(segmentBorders, 1);
		segmentBorders[0] = min;
	}
	
	List<Testcase> testcases = new List<Testcase>();
	List<Testcase> allTestcases = new List<Testcase>();
	for (int i = 1; i < segmentBorders.Length; ++i)
	{
		// Construct new Test Cases
		testcases.Clear();
		for (int j = 0; j < amount; ++j)
		{
			var test = Tester.NewTest(segmentBorders[i - 1], segmentBorders[i]);
			testcases.Add(test);
			allTestcases.Add(test);
		}

		// For all engines, run all the testcases and save the benchmark in the list for the engine.
		// If not list yet exists, make one.
		for (int k = 0; k < engines.Count; ++k)
		{
			string engine = engines[k];
			foreach (Testcase testcase in testcases)
			{
				testcase.Run(false, engine);
			}

			if (k >= benchmarkResults.Count) benchmarkResults.Add(new List<BenchmarkResult>());
			benchmarkResults[k].Add(new BenchmarkResult(testcases, segmentBorders[i - 1], segmentBorders[i], engine));
		}

	}

	// At the end, for all tests, include a benchmark for all tests.
	// Unfortunately, since we need the results for the current engine and I'm too lazy to save them too
	// We're doing all the tests again
	for (int k = 0; k < engines.Count; ++k)
	{
		foreach (Testcase testcase in testcases)
		{
			testcase.Run(false, engines[k]);
		}
		benchmarkResults[k].Insert(0, new BenchmarkResult(allTestcases, min, maxima[-1], engines[k]));
	}
	return benchmarkResults;
}

struct BenchmarkResult
{
	public readonly int MinTime, MaxTime, MinSpace, MaxSpace;
	public readonly int TotalQueries, TotalTests;
	public readonly double AverageTime, AverageSpacePerBit, AverageBvLength;
	List<NumberSpan> FailedSpans;
	public readonly string Engine;

	private int _MinLength, _MaxLength;

	// Not used
	private BenchmarkResult(int minTime, int maxTime, int avgTime, int minSpace, int maxSpace, int avgSpace, string engine)
	{
		this.MinTime = minTime;
		this.MaxTime = maxTime;
		this.AverageTime = avgTime;
		this.MinSpace = minSpace;
		this.MaxSpace = maxSpace;
		this.AverageSpacePerBit = avgSpace;
		this.AverageBvLength = 0;
		this.Engine = engine;
		this.TotalTests = 0;
		this.TotalQueries = 0;
		this._MinLength = 0;
		this._MaxLength = 0;
		this.FailedSpans = new List<NumberSpan>();
	}
	
	public BenchmarkResult(List<Testcase> testcases, int minLength, int maxLength, string engine)
	{
		this.Engine = engine;
		_MinLength = minLength;
		_MaxLength = maxLength;
		FailedSpans = new List<NumberSpan>();
		
		if (testcases.Count == 0)
		{
			MinTime = 0;
			MaxTime = 0;
			AverageTime = 0;
			MinSpace = 0;
			MaxSpace = 0;
			AverageSpacePerBit = 0;
			AverageBvLength = 0;
			TotalTests = 0;
			TotalQueries = 0;
			return;
		}
		
		int minTime = int.MaxValue;
		int maxTime = int.MinValue;
		int minSpace = int.MaxValue;
		int maxSpace = int.MinValue;
		double sumTime = 0, sumSpacePerBit = 0, sumBvLength = 0;

		int i = 0;
		foreach (Testcase testcase in testcases)
		{
			++i;
			if (testcase.Time < minTime) minTime = testcase.Time;
			else if (testcase.Time > maxTime) maxTime = testcase.Time;
			
			if (testcase.Space < minSpace) minSpace = testcase.Space;
			else if (testcase.Space > maxSpace) maxSpace = testcase.Space;
			
			sumTime += testcase.Time;
			sumSpacePerBit += (double) testcase.Space / testcase.Bitvector.Length;
			sumBvLength += testcase.Bitvector.Length;
			TotalQueries += testcase.QueryCount;
			TotalTests++;
			
			// Number Spans
			if (testcase.FailedTests > 0)
			{
				if (FailedSpans.Count == 0)
				{
					FailedSpans.Add(new NumberSpan(i));
					continue;
				}
				if (FailedSpans[-1].Add(i)) continue;
				FailedSpans.Add(new NumberSpan(i));
			}
		}
		
		this.MinTime = minTime;
		this.MaxTime = maxTime;
		this.AverageTime = sumTime / TotalTests;
		this.MinSpace = minSpace;
		this.MaxSpace = maxSpace;
		this.AverageSpacePerBit = sumSpacePerBit / TotalTests;
		this.AverageBvLength = sumBvLength / TotalTests;
	}

	public override string ToString()
	{
		StringBuilder builder = new StringBuilder();
		builder.AppendLine("---------------------- Benchmark Result ----------------------")
			.AppendLine()
			.AppendLine($"Engine: {Engine}")
			.AppendLine($"Tests Performed: {TotalTests}")
			.AppendLine($"Queries Performed: {TotalQueries}")
			.AppendLine($"Bitvector Length: {_MinLength} - {_MaxLength}")
			.AppendLine($"Average Bitvector Length: {AverageBvLength}")
			.AppendLine($"Failed Tests: {NumberSpan.ToString(FailedSpans)}")
			.AppendLine()
			.AppendLine("------- Time Benchmarks -------")
			.AppendLine($"Average Time: {AverageTime}ms")
			.AppendLine($"Shortest Time: {MinTime}ms")
			.AppendLine($"Longest Time: {MaxTime}ms")
			.AppendLine()
			.AppendLine("------- Space Benchmarks (per bit) -------")
			.AppendLine($"Average Space: {AverageSpacePerBit} bit")
			.AppendLine($"Minimal Space: {MinSpace} bit")
			.AppendLine($"Maximal Space: {MaxSpace} bit")
			.AppendLine()
			.AppendLine("------------------- End of Benchmark Result -------------------")
			.AppendLine();
		return builder.ToString();
	}

	class NumberSpan
	{
		public int Start { get; private set; }
		public int End { get; private set; }

		public NumberSpan(int number)
		{
			Start = number;
			End = number;
		}

		/// <summary>
		/// Adds a number to the span. Will only accept the Number End+1. If the number is End+1,
		/// will return true. Otherwise will return false, indicating a break in the Span and
		/// implying the user will need to create a new Span object to keep the new number.
		/// End will not be updated in the latter case.
		/// </summary>
		/// <param name="number"></param>
		/// <returns></returns>
		public bool Add(int number)
		{
			if (number != End + 1) return false;
			End++;
			return true;
		}

		public override string ToString()
		{
			return Start == End ? $"{Start}" : $"{Start} - {End}";
		}

		public static String ToString(List<NumberSpan> spans)
		{
			if (spans.Count == 0) return "-";
			StringBuilder builder = new StringBuilder();
			for (int i = 0; i < spans.Count; ++i)
			{
				builder.Append(spans[i].ToString());
				if (i < spans.Count - 1) builder.Append(", ");
			}
			return builder.ToString();
		}
	}
}