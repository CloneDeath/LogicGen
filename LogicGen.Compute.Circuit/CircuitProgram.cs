using System;
using System.Collections.Generic;
using System.Linq;
using LogicGen.Compute.ProgramCreation;

namespace LogicGen.Compute.Circuit;

public class CircuitProgram : IDisposable {
	private readonly int? _maxIterations;
	private readonly int _circuitSize;
	private readonly ComputeProgram _program;
	private readonly ComputeBuffer _inputBuffer;
	private readonly ComputeBuffer[] _stateBuffers;

	public CircuitProgram(bool[,] circuit, int? maxIterations = null) {
		_maxIterations = maxIterations;
		_circuitSize = circuit.GetLength(0);
		var circuitData = new List<byte>()
			.Concat(BitConverter.GetBytes(_circuitSize))
			.Concat(GetBytes(circuit))
			.ToArray();
		var circuitShader = new CircuitShader(_circuitSize);
		var setInputsShader = new SetInputsShader(_circuitSize);
		
		var circuitBuffer = new ComputeBuffer((uint)circuitData.Length);
		var stateBuffer1 = new ComputeBuffer(circuitShader.Buffers[1].Size);
		var stateBuffer2 = new ComputeBuffer(circuitShader.Buffers[1].Size);
		_inputBuffer = new ComputeBuffer(setInputsShader.Buffers[0].Size);

		_stateBuffers = new[] { stateBuffer1, stateBuffer2 };

		var stages = new List<IStage>();
		for (var i = 0; i < (maxIterations ?? _circuitSize); i++) {
			var source = _stateBuffers[i % 2];
			var output = _stateBuffers[(i + 1) % 2];
			
			stages.Add(new Stage(setInputsShader) {
				Bindings = new[] {
					new BufferBinding(0, _inputBuffer),
					new BufferBinding(1, source)
				}
			});
			stages.Add(new Stage(circuitShader) {
				Bindings = new[] {
					new BufferBinding(0, circuitBuffer),
					new BufferBinding(1, source),
					new BufferBinding(2, output)
				}
			});
		}
		_program = new ComputeProgram(stages.ToArray());
		_program.Upload(circuitBuffer, circuitData);
	}

	private static byte[] GetBytes(bool[,] circuit) {
		var width = circuit.GetLength(0);
		var height = circuit.GetLength(1);
		var data = new byte[width * height * sizeof(int)];
		for (var x = 0; x < width; x++)
		for (var y = 0; y < height; y++) {
			var index = x + y * width;
			var byteValue = BitConverter.GetBytes(circuit[x, y]);
			byteValue.CopyTo(data, index * sizeof(int));
		}
		return data;
	}

	public bool[] Execute(bool[] inputs) {
		var rawInputs = new bool[_circuitSize];
		inputs.CopyTo(rawInputs, 0);
		var inputData = new List<byte>()
			.Concat(BitConverter.GetBytes(inputs.Length))
			.Concat(rawInputs.Select(v => v ? 1 : 0).SelectMany(BitConverter.GetBytes))
			.ToArray();
		_program.Upload(_inputBuffer, inputData);

		_program.Upload(_stateBuffers[0], new byte[_stateBuffers[0].Size]);
		_program.Upload(_stateBuffers[1], new byte[_stateBuffers[1].Size]);
		
		_program.Execute();

		var outputBuffer = _stateBuffers[(_maxIterations ?? _circuitSize) % 2];
		var finalOutput = _program.DownloadArray<int>(outputBuffer).Select(v => v != 0).ToArray();
		return finalOutput;
	}

	public void Dispose() {
		_program.Dispose();
		GC.SuppressFinalize(this);
	}
}