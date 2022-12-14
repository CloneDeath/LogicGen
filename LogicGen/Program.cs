// https://content.wolfram.com/uploads/sites/13/2018/02/04-4-6.pdf

using System;
using System.Collections.Generic;
using System.Linq;
using LogicGen.LogicGates;

namespace LogicGen; 

public static class Program {
	public static void Main() {
		TestCircuit(new NotGate(), 1);
		TestCircuit(new OrGate(), 2);
		TestCircuit(new AndGate(), 2);
		TestCircuit(new NOrGate(), 2);
		TestCircuit(new NAndGate(), 2);
		TestCircuit(new XOrGate(), 2);
		TestCircuit(new XNOrGate(), 2);
	}

	public static void TestCircuit(ITestCircuit basis, int inputs) {
		var ruleSets = new List<RuleSet>();
		for (var i = 0; i < 100000; i++) {
			ruleSets.Add(RandomRuleSet.Generate(20));
		}

		CircuitResult? best = null;
		foreach (var ruleSet in ruleSets) {
			// https://en.wikipedia.org/wiki/NAND_logic
			var circuit = ruleSet.GenerateCircuit(inputs, 1, 10);
			var error = GetError(circuit, basis, inputs);
			if (best == null || error < best.Error) {
				best = new CircuitResult(ruleSet, circuit, error);
			}
			if (error <= 0) break;
		}

		Console.WriteLine($"{basis.Name} {1 - best?.Error}");
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