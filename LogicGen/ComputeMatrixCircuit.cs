using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LogicGen.Compute;
using LogicGen.Compute.ProgramCreation;
using LogicGen.Compute.ProgramCreation.Shaders;

namespace LogicGen; 

public class ComputeMatrixCircuit : ICircuit {
	private readonly int _inputCount;
	private readonly int _outputCount;
	private readonly Matrix _data;
	
	private readonly ComputeProgram _program;
	private readonly ComputeBuffer _inputBuffer;
	private readonly ComputeBuffer _outputBuffer;

	public ComputeMatrixCircuit(int inputCount, int outputCount, Matrix data) {
		_inputCount = inputCount;
		_outputCount = outputCount;
		_data = data;

		var boolCircuit = new bool[data.Size * data.Size];
		for (var x = 0; x < data.Size; x++) {
			for (var y = 0; y < data.Size; y++) {
				boolCircuit[x + (y * data.Size)] = data[x, y] != 0;
			}
		}

		var circuitData = new List<byte>()
			.Concat(BitConverter.GetBytes(data.Size))
			.Concat(boolCircuit.Select(v => v ? 1 : 0).SelectMany(BitConverter.GetBytes))
			.ToArray();
		var inputOutputSize = (uint)_data.Size * sizeof(int);
		var shader = new ShaderData {
			Code = File.ReadAllBytes("Shader/circuit.spv"),
			EntryPoint = "main",
			Buffers = new IBufferDescription[] { 
				new BufferDescription(0, (uint)circuitData.Length) ,
				new BufferDescription(1, inputOutputSize),
				new BufferDescription(2, inputOutputSize)
			},
			Workers = new GroupCount((uint)_inputCount)
		};
		var circuitBuffer = new ComputeBuffer(circuitData.Length);
		_inputBuffer = new ComputeBuffer(inputOutputSize);
		_outputBuffer = new ComputeBuffer(inputOutputSize);
		_program = new ComputeProgram(new IStage[] {
			new Stage(shader) {
				Bindings = new [] {
					new BufferBinding(0, circuitBuffer),
					new BufferBinding(1, _inputBuffer),
					new BufferBinding(2, _outputBuffer)
				}
			}
		});
		_program.Upload(circuitBuffer, circuitData);
	}

	public bool[] Execute(params bool[] inputs) {
		if (_inputCount != inputs.Length) throw new NotImplementedException();
		
		for (var circuitIteration = 0; circuitIteration < _data.Size; circuitIteration++) {
			var inputData = _program.DownloadArray<int>(_outputBuffer).Select(v => v != 0).ToArray();
			for (var inputIndex = 0; inputIndex < _inputCount; inputIndex++) {
				inputData[inputIndex] = inputs[inputIndex];
			}

			_program.Upload(_inputBuffer, inputData.Select(v => v ? 1 : 0).ToArray());
			_program.Execute();
		}
		var finalOutput = _program.DownloadArray<int>(_outputBuffer).Select(v => v != 0).ToArray();
		return finalOutput.TakeLast(_outputCount).ToArray();
	}

}