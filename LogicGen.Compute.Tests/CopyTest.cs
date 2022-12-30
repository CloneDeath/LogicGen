using System.IO;
using FluentAssertions;
using LogicGen.Compute.Shaders;

namespace LogicGen.Compute.Tests;

public class CopyTest {
	[Test]
	public void WorksWithByteArray() {
		var input = new InputData {
			BindingIndex = 0,
			Data = new byte[] { 1, 2, 3, 4, 5 }
		};
		var output = new OutputData {
			BindingIndex = 1
		};
		var shader = new ShaderData {
			Code = File.ReadAllBytes("Shader/copy.spv"),
			EntryPoint = "main",
			Buffers = new IBufferDescription[] {
				new BufferDescription(0, 5),
				new BufferDescription(1, 5)
			},
			Workers = new GroupCount((uint)input.Data.Length)
		};
		using var program = new ComputeProgram(shader);
		
		program.Execute(new IInputData[] { input }, new IOutputData[] { output });
		output.Data.Should().ContainInOrder(1, 2, 3, 4, 5);
	}
}