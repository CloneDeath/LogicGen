using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LogicGen.Compute;
using LogicGen.Compute.DataTypes;
using LogicGen.Compute.ProgramCreation.Shaders;

namespace LogicGen; 

public class ComputeMatrixCircuit : ICircuit {
	private readonly int _inputCount;
	private readonly int _outputCount;
	private readonly Matrix _data;
	
	private readonly ComputeProgram _program;
	private readonly InputData _circuitData;

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
		_circuitData = new InputData {
			BindingIndex = 0,
			Data = new List<byte>()
				.Concat(BitConverter.GetBytes(data.Size))
				.Concat(boolCircuit.Select(v => v ? 1 : 0).SelectMany(BitConverter.GetBytes))
				.ToArray()
		};
		
		var shader = new ShaderData {
			Code = File.ReadAllBytes("Shader/circuit.spv"),
			EntryPoint = "main",
			Buffers = new IBufferDescription[] { 
				new BufferDescription(0, (uint)_circuitData.Data.Length) ,
				new BufferDescription(1, (uint)_data.Size * sizeof(int)),
				new BufferDescription(2, (uint)_data.Size * sizeof(int))
			},
			Workers = new GroupCount((uint)_inputCount)
		};
		_program = new ComputeProgram(shader);
	}

	public bool[] Execute(params bool[] inputs) {
		if (_inputCount != inputs.Length) throw new NotImplementedException();
		
		var output = new BinaryOutputData(2, _data.Size);
		for (var circuitIteration = 0; circuitIteration < _data.Size; circuitIteration++) {
			var inputsData = output.BinaryData.ToArray();
			for (var inputIndex = 0; inputIndex < _inputCount; inputIndex++) {
				inputsData[inputIndex] = inputs[inputIndex];
			}
			var input = new BinaryInputData(1, inputsData);
			_program.Execute(new IInputData[] { _circuitData, input }, 
				new IOutputData[] { output });
		}

		return output.BinaryData.TakeLast(_outputCount).ToArray();
	}

}