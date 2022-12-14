// https://content.wolfram.com/uploads/sites/13/2018/02/04-4-6.pdf

using System;
using System.Collections.Generic;
using System.Linq;
using LogicGen.LogicGates;

namespace LogicGen; 

public static class Program {
	public static void Main(string[] args) {
		var ruleSets = new List<RuleSet>();
		for (var i = 0; i < 100; i++) {
			ruleSets.Add(RandomRuleSet.Generate(20));
		}

		var basis = new NotGate();
		CircuitResult? best = null;
		foreach (var ruleSet in ruleSets) {
			// https://en.wikipedia.org/wiki/NAND_logic
			var circuit = ruleSet.GenerateCircuit(1, 1, 5);
			var error = GetError(circuit, basis, 1);
			Console.WriteLine(error);

			if (best == null || error < best.Error) {
				best = new CircuitResult(ruleSet, circuit, error);
			}
		}

		Console.WriteLine(best?.Error);
	}

	private static double GetError(ICircuit circuit, ICircuit basis, int inputCount) {
		var error = 0d;
		var inputSets = GenerateInputSet(inputCount).ToList();
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

	private static IEnumerable<bool[]> GenerateInputSet(int inputCount) {
		for (var value = 0; value < Math.Pow(2, inputCount); value++) {
			var inputSet = new bool[inputCount];
			for (var digit = 0; digit < inputCount; digit++) {
				inputSet[digit] = ((value >> digit) & 0x01) == 1;
			}
			yield return inputSet;
		}
	}
}