// https://content.wolfram.com/uploads/sites/13/2018/02/04-4-6.pdf

using System;
using System.Collections.Generic;
using System.Linq;
using LogicGen.ByteOperations;
using LogicGen.LogicGates;

namespace LogicGen; 

public static class Program {
	public static void Main() {
		TestCircuit(new NotGate());
		TestCircuit(new AndGate());
		TestCircuit(new NAndGate());
		TestCircuit(new OrGate());
		TestCircuit(new NOrGate());
		TestCircuit(new XOrGate());
		TestCircuit(new XNOrGate());
		Console.WriteLine();

		TestCircuit(new AddOperation());
		TestCircuit(new SubtractOperation());
		TestCircuit(new BitwiseAndOperation());
		TestCircuit(new BitwiseOrOperation());
		TestCircuit(new MultiplyOperation());
		TestCircuit(new DivideOperation());
	}

	public static void TestCircuit(ITestCircuit basis) {
		var ruleSets = new List<RuleSet>();
		for (var i = 0; i < 100; i++) {
			ruleSets.Add(RandomRuleSet.Generate(20));
		}

		CircuitResult? best = null;
		foreach (var ruleSet in ruleSets) {
			// https://en.wikipedia.org/wiki/NAND_logic
			var circuit = ruleSet.GenerateCircuit(basis.NumberOfInputs, basis.NumberOfOutputs, 16 * 4);
			var error = GetError(circuit, basis, basis.NumberOfInputs);
			if (best == null || error < best.Error) {
				best = new CircuitResult(ruleSet, circuit, error);
			}
			if (error <= 0) break;
		}

		if (best != null && basis.Name == "Divide") {
			var inputs = new byte[] {
				10, 2
			}.SelectMany(ByteOperation.ConvertByteToBoolArray).ToArray();
			var bestOut = ByteOperation.ConvertBoolArrayToByte(best.Circuit.Execute(inputs));
			var basisOut = ByteOperation.ConvertBoolArrayToByte(basis.Execute(inputs));
			Console.WriteLine($"10/2 = Got {bestOut}, Expected {basisOut}.");
		}

		Console.WriteLine($"{basis.Name} {1 - best?.Error}");
	}

	private static double GetError(ICircuit circuit, ICircuit basis, int inputCount) {
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

	private static IEnumerable<bool[]> GenerateInputSet(int inputCount) {
		for (var value = 0; value < Math.Pow(2, inputCount); value++) {
			var inputSet = new bool[inputCount];
			for (var digit = 0; digit < inputCount; digit++) {
				inputSet[digit] = ((value >> digit) & 0x01) == 1;
			}
			yield return inputSet;
		}
	}

	private static readonly Random _random = new();

	private static IEnumerable<bool[]> GenerateRandomInputSet(int inputCount) {
		var numberOfCases = Math.Max(Math.Pow(2, inputCount) / 10, 10);
		for (var value = 0; value < numberOfCases; value++) {
			var inputSet = new bool[inputCount];
			for (var digit = 0; digit < inputCount; digit++) {
				inputSet[digit] = _random.Next(2) == 1;
			}
			yield return inputSet;
		}
	}
}