using System.IO;
using FluentAssertions;

namespace LogicGen.Compute.Tests;

public class CopyBitsTest {
	[Test]
	public void WorksWithByteArray() {
		var code = File.ReadAllBytes("Shader/copy_bits.spv");
		var program = new ComputeProgram(code, "main");
		var output = program.Execute(new byte[]{1, 2, 3, 4, 5});
		output.Should().ContainInOrder(1, 2, 3, 4, 5);
	}
}