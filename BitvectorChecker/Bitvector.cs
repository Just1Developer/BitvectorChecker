#define EXCLUSIVE_RANK

using System.Text;

namespace BitvectorChecker;

public class Bitvector
{
	private const int CacheFrequencyShift = 10;
	private const int CacheFrequency = 1 << CacheFrequencyShift;
	
	private List<byte> vector;
	private Dictionary<long, long> rank_1;
	private Dictionary<long, long> rank_0;
	private Dictionary<long, long> select_0;
	private Dictionary<long, long> select_1;

	private const bool USE_CACHE = true;
	private const bool SPARSE_CACHE = false;
	
	public Bitvector(string vect)
	{
		vector = new();
		rank_0 = new Dictionary<long, long>();
		rank_1 = new Dictionary<long, long>();
		select_0 = new Dictionary<long, long>();
		select_1 = new Dictionary<long, long>();
		ReadVector(vect);
	}

	public byte Access(long pos)
	{
		return vector[(int) pos];
	}

	// RANK IS EXCLUSIVE!!! (at least it's supposed to be)
	
	public long Rank(long num, long pos)
	{
		long r;

		if (USE_CACHE)
		{
			if (!SPARSE_CACHE)
			{
				r = (num == 0 ? rank_0 : rank_1).GetValueOrDefault(pos, -1);
				if (r >= 0 && USE_CACHE) return r;
			}
			long cacheIndex = (pos >> CacheFrequencyShift) - 1;
			long remaining = pos & (CacheFrequency - 1);
			r = (num == 0 ? rank_0 : rank_1).GetValueOrDefault(pos, 0);

			if (cacheIndex >= 0)
			{
				for (int i = 0; i < remaining; ++i)
				{
					if (cacheIndex + i >= vector.Count)
					{
						Console.WriteLine(
							$"ERROR: rank {num} {pos} was out of bounds for vector list size {vector.Count}.");
						return -1;
					}

					if (vector[(int)(cacheIndex + i)] == num) r++;
				}

				return r;
			}
		}
		
		long counter = 0;
		#if EXCLUSIVE_RANK
		for (long i = 0; i < Math.Min(pos, vector.Count - 1); i++)
		#else
		for (long i = 0; i <= Math.Min(pos, vector.Count - 1); i++)
		#endif
		{
			if (i >= vector.Count)
			{
				Console.WriteLine($"ERROR: rank {num} {pos} was out of bounds for vector list size {vector.Count}.");
				return -1;
			}
			if (vector[(int) i] == num) counter++;
		}
		return counter;
	}

	public long Select(long num, long pos)
	{
		long r;

		if (USE_CACHE)
		{
			if (!SPARSE_CACHE)
			{
				r = (num == 0 ? select_0 : select_1).GetValueOrDefault(pos, -1);
				if (r >= 0 && USE_CACHE) return r;
			}
			long cacheIndex = (pos >> (CacheFrequencyShift)) - 1;
			r = (num == 0 ? rank_0 : rank_1).GetValueOrDefault(pos, 0);

			if (cacheIndex >= 0)
			{
				int i = 0;
				while (r < pos)
				{
					if (cacheIndex + i >= vector.Count)
					{
						Console.WriteLine(
							$"ERROR: select {num} {pos} was out of bounds for vector list size {vector.Count}.");
						return -1;
					}

					if (vector[(int)(cacheIndex + i)] == num) r++;
					++i;
				}

				return r;
			}
		}
		
		long counter = 0;
		long _i = -1;
		while (counter < pos)
		{
			if (_i >= vector.Count - 1)
			{
				Console.WriteLine($"ERROR: select {num} {pos} was out of bounds for vector list size {vector.Count}.");
				return -1;
			}
			if (vector[(int) ++_i] == num) counter++;
		}
		return _i;
	}

	private void ReadVector(string s)
	{
		long ones = 0, zeros = 0, index = 0;
		int cacheIndex = 0;
		foreach (char c in s)
		{
			vector.Add((byte) (c - '0'));
			++cacheIndex;
			
			if (USE_CACHE && (!SPARSE_CACHE || cacheIndex >= CacheFrequency))
			{
				cacheIndex = 0;
				rank_0.Add(index, zeros);
				rank_1.Add(index, ones);
				if (c == '1')
				{
					ones++;
					select_1.Add(ones, index);
				}
				else if (c == '0')
				{
					zeros++;
					select_0.Add(zeros, index);
				}
			}
			else
			{
				if (c == '1') ones++;
				else zeros++;
			}
			index++;
		}
		Console.WriteLine($"Read Vector, got {ones} Ones and {zeros} Zeros, Index: {index}.");
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