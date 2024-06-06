// See https://aka.ms/new-console-template for more information

using BitvectorChecker;

Console.WriteLine("Hello, World!");

/*
string[] commands = {
	"001110110101010111111111",
	"access 4",         // Correct output
	"rank 0 10",        // Todo incorrect output: 4
	"rank 0 11",        // 
	"rank 0 12",        // 
	"rank 1 10",        // 
	"rank 1 11",        // 
	"rank 1 12",        // 
	"select 1 14",      // Correct output
	"rank 1 10",        // Correct output
	"select 0 3",       // Correct output
	"access 5",         // Correct output
};
int _commands = commands.Length - 1;

Bitvector bitvector = new Bitvector(commands[0]);
for (int i = 0; i < _commands; ++i) {
	// Todo just next command
	ProcessCommand(commands[i + 1], bitvector);
}*/

List<Testcase> testcases = new List<Testcase>();
for (int i = 0; i < 5; ++i)
{
	testcases.Add(Tester.NewTest(10, 30));
}

foreach (Testcase testcase in testcases)
{
	await testcase.Run(true);
}

static void ProcessCommand(string cmd, Bitvector vect)
{
	string[] str = cmd.Split(" ");
	
	char command = str[0][0];
	int arg1 = int.Parse(str[1]);
	int arg2 = str.Length > 2 ? int.Parse(str[2]) : 0;

	switch (command) {
		case 'a':
			Console.WriteLine(vect.Access(arg1));
			return;
		case 'r':
			Console.WriteLine(vect.Rank(arg1, arg2));
			return;
		case 's':
			Console.WriteLine(vect.Select(arg1, arg2));
			return;
		default:
			return;
	}
}