using System.IO;
using FluentAssertions;

namespace LogicGen.Compute.Tests;

public class CopyTest {
	[Test]
	public void WorksWithByteArray() {
		var code = File.ReadAllBytes("Shader/copy.spv");
		var program = new ComputeProgram(code, "main");
		var input = new InputData {
			BindingIndex = 0,
			Data = new byte[] { 1, 2, 3, 4, 5 }
		};
		var output = new OutputData {
			BindingIndex = 1,
			Size = input.Data.Length
		};
		program.Execute(new IInputData[] { input }, 
			new IOutputData[] { output }, 
			new GroupCount((uint)input.Data.Length));
		output.Data.Should().ContainInOrder(1, 2, 3, 4, 5);
	}
}