using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using LogicGen.Compute.ProgramCreation;
using LogicGen.Compute.ProgramCreation.Shaders;

namespace LogicGen.Compute.Tests; 

public class CircuitTest {
	[TestCase(false, false, false)]
	[TestCase(false, true,  false)]
	[TestCase(true,  false, false)]
	[TestCase(true,  true,  true)]
	public void WorksWithAndCircuit(bool a, bool b, bool expected) {
		const int circuitSize = 4;
		var AndCircuit = new[] {
			false, false, true, false,
			false, false, true, false,
			false, false, false, true,
			false, false, false, false
		};
		var circuitData = new List<byte>()
			.Concat(BitConverter.GetBytes(circuitSize))
			.Concat(AndCircuit.Select(v => v ? 1 : 0).SelectMany(BitConverter.GetBytes))
			.ToArray();
		const uint inputOutputSize = circuitSize * sizeof(int);
		var circuitBuffer = new ComputeBuffer((uint)circuitData.Length);
		var inputBuffer = new ComputeBuffer(inputOutputSize);
		var outputBuffer = new ComputeBuffer(inputOutputSize);
		var shader = new ShaderData {
			Code = File.ReadAllBytes("Shader/circuit.spv"),
			EntryPoint = "main",
			Buffers = new IBufferDescription[] {
				new BufferDescription(0, (uint)circuitData.Length),
				new BufferDescription(1, inputOutputSize),
				new BufferDescription(2, inputOutputSize)
			},
			Workers = new GroupCount(4)
		};
		using var program = new ComputeProgram(new IStage[] {
			new Stage(shader) {
				Bindings = new [] {
					new BufferBinding(0, circuitBuffer),
					new BufferBinding(1, inputBuffer),
					new BufferBinding(2, outputBuffer)
				}
			}
		});
		program.Upload(circuitBuffer, circuitData);
		
		for (var i = 0; i < circuitSize; i++) {
			var inputs = program.DownloadArray<int>(outputBuffer).Select(v => v != 0).ToArray();
			inputs[0] = a;
			inputs[1] = b;

			program.Upload(inputBuffer, inputs.Select(v => v ? 1 : 0).ToArray());
			
			program.Execute();

			var output = program.DownloadArray<int>(outputBuffer).Select(v => v != 0).ToArray();
			Console.WriteLine(
				string.Join(" ", inputs)
				+ " -> "
				+ string.Join(" ", output));
		}

		var finalOutput = program.DownloadArray<int>(outputBuffer).Select(v => v != 0).ToArray();
		finalOutput[3].Should().Be(expected);
	}
}