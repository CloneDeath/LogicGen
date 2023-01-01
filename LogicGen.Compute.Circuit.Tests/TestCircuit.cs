using NUnit.Framework;

namespace LogicGen.Compute.Circuit.Tests;

public class TestCircuit {
	[TestCase(false, false, false)]
	[TestCase(false, true,  false)]
	[TestCase(true,  false, false)]
	[TestCase(true,  true,  true)]
	public void ANDCircuitWorks(bool a, bool b, bool expected) {
		var AndCircuit = new[] {
			false, false, true, false,
			false, false, true, false,
			false, false, false, true,
			false, false, false, false
		};
		var program = new CircuitProgram(AndCircuit);
		var inputs = new[] { a, b };
		var output = program.Execute(inputs);
		output[3].Should.Be(expected);
	}
}