// See https://aka.ms/new-console-template for more information

using BitvectorChecker;

TestMultiple(200, 10, 60);

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
		testcase.Run(false);
	}
}

static List<BenchmarkResult> BenchmarkMultiple(int amount, int min, int[] maxima)
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
			testcase.Run(false);
		}

		benchmarkResults.Add(new BenchmarkResult(testcases));
	}
	benchmarkResults.Insert(0, new BenchmarkResult(allTestcases));
	return benchmarkResults;
}

struct BenchmarkResult
{
	public readonly int MinTime, MaxTime, AverageTime, MinSpace, MaxSpace, AverageSpace;

	public BenchmarkResult(int minTime, int maxTime, int avgTime, int minSpace, int maxSpace, int avgSpace)
	{
		this.MinTime = minTime;
		this.MaxTime = maxTime;
		this.AverageTime = avgTime;
		this.MinSpace = minSpace;
		this.MaxSpace = maxSpace;
		this.AverageSpace = avgSpace;
	}
	
	public BenchmarkResult(List<Testcase> testcases)
	{
		if (testcases.Count == 0)
		{
			MinTime = 0;
			MaxTime = 0;
			AverageTime = 0;
			MinSpace = 0;
			MaxSpace = 0;
			AverageSpace = 0;
			return;
		}
		
		int minTime = int.MaxValue;
		int maxTime = int.MinValue;
		int minSpace = int.MaxValue;
		int maxSpace = int.MinValue;
		double sumTime = 0, sumSpace = 0;

		foreach (Testcase testcase in testcases)
		{
			if (testcase.Time < minTime) minTime = testcase.Time;
			else if (testcase.Time > maxTime) maxTime = testcase.Time;
			
			if (testcase.Space < minSpace) minSpace = testcase.Space;
			else if (testcase.Space > maxSpace) maxSpace = testcase.Space;
			
			sumTime += testcase.Time;
			sumSpace += testcase.Space;
		}
		
		
		
		this.MinTime = minTime;
		this.MaxTime = maxTime;
		this.AverageTime = (int) Math.Round(sumTime / testcases.Count);
		this.MinSpace = minSpace;
		this.MaxSpace = maxSpace;
		this.AverageSpace = (int) Math.Round(sumSpace / testcases.Count);
	}
}