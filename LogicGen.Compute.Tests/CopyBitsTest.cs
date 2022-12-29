using System.IO;
using FluentAssertions;

namespace LogicGen.Compute.Tests;

public class CopyBitsTest {
	[Test]
	public void WorksWithBoolArray() {
		var code = File.ReadAllBytes("Shader/copy_bits.spv");
		var program = new ComputeProgram(code, "main");
		var input = new InputData {
			BindingIndex = 0,
			Data = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }
		};
		var output = new OutputData {
			BindingIndex = 1,
			Size = input.Data.Length
		};
		program.Execute(new[] { input }, new[] { output }, new GroupCount(8 * (uint)input.Data.Length));
		output.Data.Should().ContainInOrder(1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
	}
}