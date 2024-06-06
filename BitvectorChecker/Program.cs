// See https://aka.ms/new-console-template for more information

using BitvectorChecker;

TestMultiple(1000, 10, 150);

// ------------- Methods -------------

static bool TestSingleFile(int inputFileName, bool logAll)
{
	return new Testcase($"./input/{inputFileName}.in").Run(logAll);
}

static void TestMultiple(int amount, int min = 15, int max = 60)
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