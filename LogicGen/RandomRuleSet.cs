using System;
using System.Collections.Generic;
using System.Linq;

namespace LogicGen; 

public static class RandomRuleSet {
	private static readonly Random _random = new();
	public static RuleSet Generate(int numberOfRules) {
		if (numberOfRules is < 1 or > 26) throw new NotImplementedException();

		var totalNumberOfRules = numberOfRules + IndexToName.StartIndex;

		var variableRules = new List<Rule>();
		for (var i = 0; i < numberOfRules; i++) {
			var index = IndexToName.StartIndex + i;
			var matrix = GenerateMatrix(totalNumberOfRules);
			variableRules.Add(new Rule(index, matrix));
		}

		var rules = RuleSet.GetConstantRules().Concat(variableRules);
		return new RuleSet(rules.ToList());
	}

	private static Matrix GenerateMatrix(int totalNumberOfRules) {
		var data = new[,] {
			{ _random.Next(totalNumberOfRules), _random.Next(totalNumberOfRules) },
			{ _random.Next(totalNumberOfRules), _random.Next(totalNumberOfRules) }
		};
		return new Matrix(data);
	}
}