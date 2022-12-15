using System;

namespace LogicGen; 

public class RandomProvider {
	private readonly Random _random = new();

	public int Next(int maximumExclusive) => _random.Next(maximumExclusive);

	public bool NextBool() => _random.Next(2) == 1;

	public bool ChanceHappens(double probability) {
		var actual = _random.NextDouble();
		return actual < probability;
	}
}