namespace BitvectorChecker;

public class Bitvector
{
	private List<int> vector;
	
	public Bitvector(string vect)
	{
		vector = new();
		ReadVector(vect);
	}

	public int Access(int pos)
	{
		return vector[pos];
	}

	// RANK IS EXCLUSIVE!!! (at least it's supposed to be)
	
	public int Rank(int num, int pos)
	{
		int counter = 0;
		for (int i = 0; i < Math.Min(pos, vector.Count - 1); i++)
		{
			if (vector[i] == num) counter++;
		}
		return counter;
	}

	public int Select(int num, int pos)
	{
		int counter = 0;
		int i = -1;
		while (counter < pos)
		{
			if (vector[++i] == num) counter++;
		}
		return i;
	}

	private void ReadVector(string s)
	{
		foreach (char c in s)
		{
			vector.Add(c - '0');
		}
	}
}