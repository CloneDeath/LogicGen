using System;
using System.Linq;

namespace LogicGen; 

public class MatrixCircuit : ICircuit {
	private readonly int _inputs;
	private readonly int _outputs;
	private readonly Matrix _data;

	public MatrixCircuit(int inputs, int outputs, Matrix data) {
		_inputs = inputs;
		_outputs = outputs;
		_data = data;
	}

	public bool[] Execute(params bool[] inputs) {
		if (_inputs != inputs.Length) throw new NotImplementedException();

		var result = new bool[_outputs];
		for (var i = 0; i < _outputs; i++) {
			var index = _data.Size - i - 1;
			result[i] = GetOutput(index, inputs);
		}
		return result;
	}

	private bool GetOutput(int index, bool[] inputs) {
		if (index < inputs.Length) return inputs[index];
		var sourceIndexes = _data.GetInputsFor(index);
		var sourceValues = sourceIndexes.Select(i => GetOutput(i, inputs));
		return !sourceValues.All(i => i); // NAND
	}
}