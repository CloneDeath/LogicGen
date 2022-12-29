using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using LogicGen.Compute.DataTypes;

namespace LogicGen.Compute.Tests; 

public class CircuitTest {
	[TestCase(false, false, false)]
	[TestCase(false, true,  false)]
	[TestCase(true,  false, false)]
	[TestCase(true,  true,  true)]
	public void WorksWithAndCircuit(bool a, bool b, bool expected) {
		var shader = new ShaderData {
			Code = File.ReadAllBytes("Shader/circuit.spv"),
			EntryPoint = "main"
		};
		const int circuitSize = 4;
		var AndCircuit = new[] {
			false, false, true, false,
			false, false, true, false,
			false, false, false, true,
			false, false, false, false
		};
		var circuitData = new InputData {
			BindingIndex = 0,
			Data = new List<byte>()
				.Concat(BitConverter.GetBytes(circuitSize))
				.Concat(AndCircuit.Select(v => v ? 1 : 0).SelectMany(BitConverter.GetBytes))
				.ToArray()
		};
		
		using var program = new ComputeProgram(shader,new GroupCount(4),
			new DataDescription(0, (uint)circuitData.Data.Length),
			new DataDescription(1, circuitSize * sizeof(int)),
			new DataDescription(2, circuitSize * sizeof(int)));
		
		var output = new BinaryOutputData(2, circuitSize);
		for (var i = 0; i < circuitSize; i++) {
			var inputs = output.BinaryData.ToArray();
			inputs[0] = a;
			inputs[1] = b;
			var input = new BinaryInputData(1, inputs);
			program.Execute(new IInputData[] { circuitData, input }, 
				new IOutputData[] { output });
			Console.WriteLine(
				string.Join(" ", inputs)
				+ " -> "
				+ string.Join(" ", output.BinaryData));
		}

		output.BinaryData[3].Should().Be(expected);
	}
}