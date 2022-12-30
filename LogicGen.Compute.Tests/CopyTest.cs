using System.IO;
using FluentAssertions;
using LogicGen.Compute.ProgramCreation;
using LogicGen.Compute.ProgramCreation.Shaders;

namespace LogicGen.Compute.Tests;

public class CopyTest {
	[Test]
	public void WorksWithByteArray() {
		var inputData = new byte[] { 1, 2, 3, 4, 5 };
		var inputBuffer = new ComputeBuffer(inputData.Length);
		var outputBuffer = new ComputeBuffer(inputData.Length);
		var shader = new ShaderData {
			Code = File.ReadAllBytes("Shader/copy.spv"),
			EntryPoint = "main",
			Buffers = new IBufferDescription[] {
				new BufferDescription(0, inputData.Length),
				new BufferDescription(1, inputData.Length)
			},
			Workers = new GroupCount((uint)inputData.Length)
		};
		using var program = new ComputeProgram(new IStage[] {
			new Stage(shader) {
				Bindings = new[] {
					new BufferBinding(0, inputBuffer),
					new BufferBinding(1, outputBuffer)
				}
			}
		});

		program.Upload(inputBuffer, inputData);
		
		program.Execute();

		var outputData = program.Download(outputBuffer);
		outputData.Should().ContainInOrder(1, 2, 3, 4, 5);
	}
}