// https://content.wolfram.com/uploads/sites/13/2018/02/04-4-6.pdf

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using LogicGen.ByteOperations;
using LogicGen.Genetics;
using LogicGen.LogicGates;

namespace LogicGen; 

public static class Program {
	public static void Main() {
		// https://en.wikipedia.org/wiki/NAND_logic
		/*TestCircuit(new NotGate());
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
		TestCircuit(new MultiplyOperation());*/
		var stopwatch = Stopwatch.StartNew();
		TestCircuit(new DivideOperation());
		Console.WriteLine($"Total Time: {stopwatch.Elapsed}");
	}

	public static void TestCircuit(ITestCircuit basis) {
		Console.WriteLine($"Running for {basis.Name}");

		var random = new RandomProvider();
		var population = new Population(100, 20, random);
		Console.WriteLine("Population Generated");

		for (var i = 0; i < 10; i++) {
			Console.WriteLine("Advancing Generation");
			population.AdvanceGeneration();

			Console.WriteLine("Testing Generation");
			for (var index = 0; index < population.RuleSetsResults.Count; index++) {
				var ruleSetResult = population.RuleSetsResults[index];
				Console.WriteLine($"\tTesting {index}");
				ruleSetResult.CompareAgainst(basis, 16 * 40);
			}

			Console.WriteLine("Testing Manually");
			var best = population.RuleSetsResults.OrderBy(r => r.Error).First();
			if (best.Circuit != null && basis.Name == "Divide") {
				var inputs = new byte[] {
					10, 2
				}.SelectMany(ByteOperation.ConvertByteToBoolArray).ToArray();
				var bestOut = ByteOperation.ConvertBoolArrayToByte(best.Circuit.Execute(inputs));
				var basisOut = ByteOperation.ConvertBoolArrayToByte(basis.Execute(inputs));
				Console.WriteLine($"Error: {best.Error}");
				Console.WriteLine($"10/2 = Got {bestOut}, Expected {basisOut}.");
			}

			if (best.Error <= 0) {
				Console.WriteLine("Zero Error Found");
				break;
			}
		}

		var bestResult = population.RuleSetsResults.OrderBy(r => r.Error).First();
		Console.WriteLine($"{basis.Name} {1 - bestResult.Error}");
	}

}