#define EXCLUSIVE_RANK

using System.Text;

namespace BitvectorChecker;

public class Bitvector : IBitvector
{
	private const int CacheFrequencyShift = 4; // 10
	private const int CacheFrequency = 1 << CacheFrequencyShift;
	private const int CacheFrequencyMask = CacheFrequency - 1;
	private const int CacheFrequencyInverseMask = ~CacheFrequencyMask;
	
	private LargeList<byte> vector;
	private LargeDictionary<long, long> rank_1;
	private LargeDictionary<long, long> rank_0;
	private LargeDictionary<long, long> select_0;
	private LargeDictionary<long, long> select_1;

	private const bool USE_CACHE = true;
	private const bool SPARSE_CACHE = false;
	
	public Bitvector(string vect)
	{
		vector = new();
		rank_0 = new LargeDictionary<long, long>();
		rank_1 = new LargeDictionary<long, long>();
		select_0 = new LargeDictionary<long, long>();
		select_1 = new LargeDictionary<long, long>();
		ReadVector(vect);
	}

	public override byte Access(long pos)
	{
		return vector[(int) pos];
	}

	// RANK IS EXCLUSIVE!!! (at least it's supposed to be)
	
	public override long Rank(long num, long pos)
	{
		long r;

		if (USE_CACHE)
		{
			if (!SPARSE_CACHE)
			{
				r = (num == 0 ? rank_0 : rank_1).GetValueOrDefault(pos, -1);
				if (r >= 0 && USE_CACHE) return r;
			}

			long cacheIndex = pos & CacheFrequencyInverseMask; // (pos >> CacheFrequencyShift) - 1;
			long remaining = pos & CacheFrequencyMask;
			r = cacheIndex <= 0 ? 0 : (num == 0 ? rank_0 : rank_1).GetValueOrDefault(pos, 0);

			if (cacheIndex >= 0)
			{
				for (int i = 0; i < remaining; ++i)
				{
					if (cacheIndex + i >= vector.Count)
					{
						Console.WriteLine(
							$"ERROR: rank {num} {pos} was out of bounds for vector LargeList size {vector.Count}.");
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
				Console.WriteLine($"ERROR: rank {num} {pos} was out of bounds for vector LargeList size {vector.Count}.");
				return -1;
			}
			if (vector[(int) i] == num) counter++;
		}
		return counter;
	}

	public override long Select(long num, long pos)
	{
		long r;

		if (USE_CACHE)
		{
			if (!SPARSE_CACHE)
			{
				r = (num == 0 ? select_0 : select_1).GetValueOrDefault(pos, -1);
				if (r >= 0 && USE_CACHE) return r;
			}

			long cacheIndex = pos & CacheFrequencyInverseMask; //(pos >> (CacheFrequencyShift));
			r = cacheIndex == 0 ? 0 : (num == 0 ? select_0 : select_1).GetValueOrDefault(pos, 0);

			if (cacheIndex >= 0)
			{
				int i = 0;
				while (r < pos)
				{
					if (cacheIndex + i >= vector.Count)
					{
						Console.WriteLine(
							$"ERROR: select {num} {pos} was out of bounds for vector LargeList size {vector.Count}.");
						return -1;
					}

					if (vector[(int)(cacheIndex + i)] == num) r++;
					++i;
				}

				return --i;
			}
		}
		
		long counter = 0;
		long _i = -1;
		while (counter < pos)
		{
			if (_i >= vector.Count - 1)
			{
				Console.WriteLine($"ERROR: select {num} {pos} was out of bounds for vector LargeList size {vector.Count}.");
				return -1;
			}
			if (vector[(int) ++_i] == num) counter++;
		}
		return _i;
	}

	protected override void ReadVector(string s)
	{
		long ones = 0, zeros = 0, index = 0;
		int cacheIndex = 0, cacheIndex2 = 0;
		foreach (char c in s)
		{
			vector.Add((byte) (c - '0'));
			++cacheIndex;
			++cacheIndex2;
			
			if (USE_CACHE && (!SPARSE_CACHE || cacheIndex >= CacheFrequency))
			{
				cacheIndex = 0;
				rank_0.Add(SPARSE_CACHE ? cacheIndex2 : index, zeros);
				rank_1.Add(SPARSE_CACHE ? cacheIndex2 : index, ones);
				if (c == '1')
				{
					ones++;
					select_1.Add(ones, SPARSE_CACHE ? cacheIndex2 : index);
				}
				else if (c == '0')
				{
					zeros++;
					select_0.Add(zeros, SPARSE_CACHE ? cacheIndex2 : index);
				}
			}
			else
			{
				if (c == '1') ones++;
				else zeros++;
			}
			index++;
		}
		Console.WriteLine($"Read Vector, got {ones} Ones and {zeros} Zeros.");
	}
	
	internal override string ProcessCommand(string cmd)
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

	public override long Length() => vector.Count;
}

public class BitvectorGpt4Improved : IBitvector
{
	private const int CacheFrequencyShift = 4; // 10
    private const int CacheFrequency = 1 << CacheFrequencyShift;
    private const int CacheFrequencyMask = CacheFrequency - 1;
    private const int CacheFrequencyInverseMask = ~CacheFrequencyMask;

    private LargeList<byte> vector;
    private LargeDictionary<long, long> rank_1;
    private LargeDictionary<long, long> rank_0;
    private LargeDictionary<long, long> select_0;
    private LargeDictionary<long, long> select_1;

    private const bool USE_CACHE = true;
    private const bool SPARSE_CACHE = true;

    public BitvectorGpt4Improved(string vect)
    {
        vector = new();
        rank_0 = new LargeDictionary<long, long>();
        rank_1 = new LargeDictionary<long, long>();
        select_0 = new LargeDictionary<long, long>();
        select_1 = new LargeDictionary<long, long>();
        ReadVector(vect);
    }

    public override byte Access(long pos)
    {
        return vector[pos];
    }

    // RANK IS EXCLUSIVE!!! (at least it's supposed to be)
    public override long Rank(long num, long pos)
    {
        long r;

        if (USE_CACHE)
        {
            if (!SPARSE_CACHE)
            {
                r = (num == 0 ? rank_0 : rank_1).GetValueOrDefault(pos, -1);
                if (r >= 0) return r;
            }

            long cacheIndex = pos & CacheFrequencyInverseMask;
            long remaining = pos & CacheFrequencyMask;
            r = cacheIndex <= 0 ? 0 : (num == 0 ? rank_0 : rank_1).GetValueOrDefault(cacheIndex, 0);

            if (cacheIndex >= 0)
            {
                for (int i = 0; i < remaining; ++i)
                {
                    if (cacheIndex + i >= vector.Count)
                    {
                        Console.WriteLine(
                            $"ERROR: rank {num} {pos} was out of bounds for vector LargeList size {vector.Count}.");
                        return -1;
                    }

                    if (vector[cacheIndex + i] == num) r++;
                }

                return r;
            }
        }

        long counter = 0;
        for (long i = 0; i < Math.Min(pos, vector.Count - 1); i++)
        {
            if (vector[i] == num) counter++;
        }
        return counter;
    }

    public override long Select(long num, long pos)
    {
        long r;

        if (USE_CACHE)
        {
            if (!SPARSE_CACHE)
            {
                r = (num == 0 ? select_0 : select_1).GetValueOrDefault(pos, -1);
                if (r >= 0) return r;
            }

            long cacheIndex = (pos - 1) & CacheFrequencyInverseMask;
            r = cacheIndex == 0 ? 0 : (num == 0 ? select_0 : select_1).GetValueOrDefault(cacheIndex, 0);

            if (cacheIndex >= 0)
            {
                int i = 0;
                while (r < pos)
                {
                    if (cacheIndex + i >= vector.Count)
                    {
                        Console.WriteLine(
                            $"ERROR: select {num} {pos} was out of bounds for vector LargeList size {vector.Count}.");
                        return -1;
                    }

                    if (vector[cacheIndex + i] == num) r++;
                    ++i;
                }

                return cacheIndex + --i;
            }
        }

        long counter = 0;
        long _i = -1;
        while (counter < pos)
        {
            if (++_i >= vector.Count)
            {
                Console.WriteLine($"ERROR: select {num} {pos} was out of bounds for vector LargeList size {vector.Count}.");
                return -1;
            }
            if (vector[_i] == num) counter++;
        }
        return _i;
    }

    protected override void ReadVector(string s)
    {
        long ones = 0, zeros = 0, index = 0;
        int cacheIndex = 0;
        foreach (char c in s)
        {
            vector.Add((byte)(c - '0'));
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
        Console.WriteLine($"Read Vector, got {ones} Ones and {zeros} Zeros.");
    }

    internal override string ProcessCommand(string cmd)
    {
        if (cmd == "") return "";
        string[] str = cmd.Split(" ");

        char command = str[0][0];
        long arg1 = long.Parse(str[1]);
        long arg2 = str.Length > 2 ? long.Parse(str[2]) : 0;

        switch (command)
        {
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
        foreach (byte i in vector) builder.Append(i);
        return builder.ToString();
    }

    public override long Length() => vector.Count;
}

public abstract class IBitvector
{
	public IBitvector() { }
	public IBitvector(string vect) { }

	public abstract byte Access(long pos);
	public abstract long Rank(long num, long pos);
	public abstract long Select(long num, long pos);
	protected abstract void ReadVector(string s);

	internal abstract string ProcessCommand(string cmd);
	public abstract long Length();
}
