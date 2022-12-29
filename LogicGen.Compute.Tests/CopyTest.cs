using System.IO;
using FluentAssertions;

namespace LogicGen.Compute.Tests;

public class CopyTest {
	[Test]
	public void WorksWithByteArray() {
		var shader = new ShaderData {
			Code = File.ReadAllBytes("Shader/copy.spv"),
			EntryPoint = "main"
		};
		var input = new InputData {
			BindingIndex = 0,
			Data = new byte[] { 1, 2, 3, 4, 5 }
		};
		var output = new OutputData {
			BindingIndex = 1
		};
		using var program = new ComputeProgram(shader, new GroupCount((uint)input.Data.Length),
			new DataDescription(0, 5),
			new DataDescription(1, 5));
		
		program.Execute(new IInputData[] { input }, new IOutputData[] { output });
		output.Data.Should().ContainInOrder(1, 2, 3, 4, 5);
	}
}