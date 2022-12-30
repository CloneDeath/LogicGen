using System;
using System.Collections.Generic;
using System.Linq;

namespace LogicGen.Genetics; 

public class RuleSetResult {
	private readonly RandomProvider _random;
	
	public RuleSet RuleSet { get; }
	public ICircuit? Circuit { get; set; }
	public double? Error { get; set; }

	public RuleSetResult(RuleSet ruleSet, RandomProvider random) {
		_random = random;
		RuleSet = ruleSet;
	}

	public void CompareAgainst(ITestCircuit basis, int intermediates) {
		Circuit = RuleSet.GenerateCircuit(basis.NumberOfInputs, basis.NumberOfOutputs, intermediates);
		Error = GetError(Circuit, basis, basis.NumberOfInputs);
	}
	
	private double GetError(ICircuit circuit, ICircuit basis, int inputCount) {
		var error = 0d;
		var inputSets = GenerateRandomInputSet(inputCount).ToList();
		foreach (var inputSet in inputSets) {
			var circuitOutput = circuit.Execute(inputSet);
			var basisOutput = basis.Execute(inputSet);
			error += GetError(circuitOutput, basisOutput);
		}
		return error / inputSets.Count;
	}

	private static double GetError(bool[] circuitOutput, bool[] basisOutput) {
		if (circuitOutput.Length != basisOutput.Length) throw new NotImplementedException();
		var errors = 0d;
		for (var i = 0; i < circuitOutput.Length; i++) {
			if (circuitOutput[i] != basisOutput[i]) errors += 1;
		}
		return errors / circuitOutput.Length;
	}

	public static IEnumerable<bool[]> GenerateInputSet(int inputCount) {
		for (var value = 0; value < Math.Pow(2, inputCount); value++) {
			var inputSet = new bool[inputCount];
			for (var digit = 0; digit < inputCount; digit++) {
				inputSet[digit] = ((value >> digit) & 0x01) == 1;
			}
			yield return inputSet;
		}
	}
	
	public IEnumerable<bool[]> GenerateRandomInputSet(int inputCount) {
		var numberOfCases = Math.Max(Math.Pow(2, inputCount) / 10, 10);
		for (var value = 0; value < numberOfCases; value++) {
			var inputSet = new bool[inputCount];
			for (var digit = 0; digit < inputCount; digit++) {
				inputSet[digit] = _random.Next(2) == 1;
			}
			yield return inputSet;
		}
	}

	public RuleSetResult GenerateCrossover(RuleSetResult other, double mutationRate) {
		var selfRules = RuleSet.VariableRules;
		var otherRules = other.RuleSet.VariableRules;
		var crossoverRules = new List<Rule>();
		var totalNumberOfRules = selfRules.Count;
		
		foreach (var selfRule in selfRules) {
			var otherRule = otherRules.First(r => r.Index == selfRule.Index);
			var crossoverMatrix = new int[2, 2];
			for (var tx = 0; tx < 2; tx++) {
				for (var ty = 0; ty < 2; ty++) {
					if (_random.ChanceHappens(mutationRate)) {
						crossoverMatrix[tx, ty] = _random.Next(totalNumberOfRules);
					}
					else {
						crossoverMatrix[tx, ty] = _random.NextBool()
							? selfRule[tx, ty]
							: otherRule[tx, ty];
					}
				}
			}
			crossoverRules.Add(new Rule(selfRule.Index, new Matrix(crossoverMatrix)));
		}

		var ruleSet = new RuleSet(crossoverRules);
		return new RuleSetResult(ruleSet, _random);
	}
}