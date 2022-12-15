using System;
using System.Collections.Generic;

namespace LogicGen; 

public static class RandomRuleSet {

	public static RuleSet Generate(int numberOfRules, RandomProvider random) {
		if (numberOfRules is < 1 or > 26) throw new NotImplementedException();

		var totalNumberOfRules = numberOfRules + IndexToName.StartIndex;

		var variableRules = new List<Rule>();
		for (var i = 0; i < numberOfRules; i++) {
			var index = IndexToName.StartIndex + i;
			var matrix = GenerateMatrix(totalNumberOfRules, random);
			variableRules.Add(new Rule(index, matrix));
		}
		
		return new RuleSet(variableRules);
	}

	private static Matrix GenerateMatrix(int totalNumberOfRules, RandomProvider random) {
		var data = new[,] {
			{ random.Next(totalNumberOfRules), random.Next(totalNumberOfRules) },
			{ random.Next(totalNumberOfRules), random.Next(totalNumberOfRules) }
		};
		return new Matrix(data);
	}
}