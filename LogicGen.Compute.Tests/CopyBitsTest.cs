using System;
using System.IO;
using System.Linq;
using FluentAssertions;

namespace LogicGen.Compute.Tests;

public class CopyBitsTest {
	[Test]
	public void WorksWithBoolArray() {
		var code = File.ReadAllBytes("Shader/copy_bits.spv");
		var program = new ComputeProgram(code, "main");
		var inputData = new[] { true, true, false, false, true };
		var input = new InputData {
			BindingIndex = 0,
			Data = inputData.Select(d => d ? 1 : 0).SelectMany(BitConverter.GetBytes).ToArray()
		};
		var output = new OutputData {
			BindingIndex = 1,
			Size = input.Data.Length
		};
		program.Execute(new[] { input }, new[] { output }, new GroupCount(8 * (uint)input.Data.Length));
		output.Data.Should().ContainInOrder(
			1, 0, 0, 0, 
			1, 0, 0, 0,
			0, 0, 0, 0, 
			0, 0, 0, 0, 
			1, 0, 0, 0);
	}
}