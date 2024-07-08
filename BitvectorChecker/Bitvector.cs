#define EXCLUSIVE_RANK

using System.Text;

namespace BitvectorChecker;

public class Bitvector
{
	private List<long> vector;
	private Dictionary<long, long> rank_1;
	private Dictionary<long, long> rank_0;
	private Dictionary<long, long> select_0;
	private Dictionary<long, long> select_1;

	private const bool USE_CACHE = false;
	
	public Bitvector(string vect)
	{
		vector = new();
		rank_0 = new Dictionary<long, long>();
		rank_1 = new Dictionary<long, long>();
		select_0 = new Dictionary<long, long>();
		select_1 = new Dictionary<long, long>();
		ReadVector(vect);
	}

	public long Access(long pos)
	{
		return vector[(int) pos];
	}

	// RANK IS EXCLUSIVE!!! (at least it's supposed to be)
	
	public long Rank(long num, long pos)
	{
		long r = (num == 0 ? rank_0 : rank_1).GetValueOrDefault(pos, -1);
		if (r >= 0 && USE_CACHE) return (long) r;
		
		long counter = 0;
		#if EXCLUSIVE_RANK
		for (long i = 0; i < Math.Min(pos, vector.Count - 1); i++)
		#else
		for (long i = 0; i <= Math.Min(pos, vector.Count - 1); i++)
		#endif
		{
			if (vector[(int) i] == num) counter++;
		}
		return counter;
	}

	public long Select(long num, long pos)
	{
		long r = (num == 0 ? select_0 : select_1).GetValueOrDefault(pos, -1);
		if (r >= 0 && USE_CACHE) return (long) r;
		
		long counter = 0;
		long i = -1;
		while (counter < pos)
		{
			if (vector[(int) ++i] == num) counter++;
		}
		return i;
	}

	private void ReadVector(string s)
	{
		long ones = 0, zeros = 0, index = 0;
		foreach (char c in s)
		{
			vector.Add(c - '0');
			if (USE_CACHE)
			{
				rank_0.Add(index, zeros);
				rank_1.Add(index, ones);
				if (c == '1')
				{
					select_1.Add(ones, index);
					ones++;
				}
				else if (c == '0')
				{
					select_0.Add(zeros, index);
					zeros++;
				}

				index++;
			}
		}
	}
	
	internal string ProcessCommand(string cmd)
	{
		if (cmd == "") return "";
		string[] str = cmd.Split(" ");
	
		char command = str[0][0];
		long arg1 = long.Parse(str[1]);
		long arg2 = str.Length > 2 ? long.Parse(str[2]) : 0;

		switch (command) {
			case 'a':
				return Access(arg1).ToString();
			case 'r':
				return Rank(arg1, arg2).ToString();
			case 's':
				return Select(arg1, arg2).ToString();
			default:
				return "";
		}
	}

	public override string ToString()
	{
		StringBuilder builder = new StringBuilder();
		foreach (long i in vector) builder.Append(i);
		return builder.ToString();
	}

	public long Length { get => vector.Count; }
}