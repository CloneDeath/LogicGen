using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using LogicGen.Compute.Shaders;

namespace LogicGen.Compute.Tests;

public class CopyBitsTest {
	[Test]
	public void WorksWithBoolArray() {
		var inputData = new[] { true, true, false, false, true };
		
		var input = new InputData {
			BindingIndex = 0,
			Data = inputData.Select(d => d ? 1 : 0).SelectMany(BitConverter.GetBytes).ToArray()
		};
		var output = new OutputData {
			BindingIndex = 1,
		};
		var shader = new ShaderData {
			Code = File.ReadAllBytes("Shader/copy_bits.spv"),
			EntryPoint = "main",
			Buffers = new IBufferDescription[] {
				new BufferDescription(0, (uint)input.Data.Length),
				new BufferDescription(1, (uint)input.Data.Length)
			},
			Workers = new GroupCount(8 * (uint)input.Data.Length)
		};
		using var program = new ComputeProgram(shader);
		
		program.Execute(new IInputData[] { input }, 
			new IOutputData[] { output });
		output.Data.Should().ContainInOrder(
			1, 0, 0, 0, 
			1, 0, 0, 0,
			0, 0, 0, 0, 
			0, 0, 0, 0, 
			1, 0, 0, 0);
	}
}