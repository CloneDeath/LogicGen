using System;
using System.Collections.Generic;
using System.Linq;

namespace LogicGen.Genetics; 

public class Population {
	private readonly int _populationSize;
	private readonly RandomProvider _random;
	private const double ReproductionRate = 0.2;
	private const double MutationRate = 0.25;

	public Population(int populationSize, int numberOfRules, RandomProvider random) {
		_random = random;
		_populationSize = populationSize;
		for (var i = 0; i < populationSize; i++) {
			var ruleSet = RandomRuleSet.Generate(numberOfRules, random);
			var ruleResult = new RuleSetResult(ruleSet, random);
			RuleSetsResults.Add(ruleResult);
		}
	}

	public List<RuleSetResult> RuleSetsResults { get; } = new();

	public void AdvanceGeneration() {
		var current = RuleSetsResults.OrderBy(r => r.Error ?? 1).ToList();
		var preservationCount = Math.Max((int)Math.Ceiling(_populationSize * ReproductionRate), 2);
		var nextGeneration = current.Take(preservationCount).ToList();
		var crossover = GenerateCrossover(nextGeneration, preservationCount);

		RuleSetsResults.Clear();
		RuleSetsResults.AddRange(nextGeneration);
		RuleSetsResults.AddRange(crossover);
	}

	private List<RuleSetResult> GenerateCrossover(IReadOnlyList<RuleSetResult> nextGeneration, int preservationCount) {
		var crossover = new List<RuleSetResult>();
		while (nextGeneration.Count + crossover.Count < _populationSize) {
			var motherIndex = _random.Next(preservationCount);
			var fatherIndex = _random.Next(preservationCount);
			if (motherIndex == fatherIndex) continue;

			var mother = nextGeneration[motherIndex];
			var father = nextGeneration[fatherIndex];
			var child = mother.GenerateCrossover(father, MutationRate);
			crossover.Add(child);
		}

		return crossover;
	}
}