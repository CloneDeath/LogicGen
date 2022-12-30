using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using LogicGen.Compute.ProgramCreation;
using LogicGen.Compute.ProgramCreation.Shaders;

namespace LogicGen.Compute.Tests;

public class CopyBitsTest {
	[Test]
	public void WorksWithBoolArray() {
		var inputData = new[] { true, true, false, false, true };
		var inputBuffer = new ComputeBuffer(sizeof(int) * inputData.Length);
		var outputBuffer = new ComputeBuffer(sizeof(int) * inputData.Length);
		var shader = new ShaderData {
			Code = File.ReadAllBytes("Shader/copy_bits.spv"),
			EntryPoint = "main",
			Buffers = new IBufferDescription[] {
				new BufferDescription(0, (uint)inputData.Length),
				new BufferDescription(1, (uint)inputData.Length)
			},
			Workers = new GroupCount(8 * (uint)inputData.Length)
		};
		using var program = new ComputeProgram(new IStage[] {
			new Stage(shader) {
				Bindings = new [] {
					new BufferBinding(0, inputBuffer),
					new BufferBinding(1, outputBuffer)
				}
			}
		});

		program.Upload(inputBuffer, inputData.Select(d => d ? 1 : 0).SelectMany(BitConverter.GetBytes).ToArray());

		program.Execute();

		var outputData = program.Download(outputBuffer);
		outputData.Should().ContainInOrder(
			1, 0, 0, 0, 
			1, 0, 0, 0,
			0, 0, 0, 0, 
			0, 0, 0, 0, 
			1, 0, 0, 0);
	}
}