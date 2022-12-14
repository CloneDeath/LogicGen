using System;
using System.Collections.Generic;
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
		var cache = new Dictionary<int, bool>();
		for (var i = 0; i < inputs.Length; i++) {
			cache[i] = inputs[i];
		}

		var result = new bool[_outputs];
		for (var i = 0; i < _outputs; i++) {
			var index = _data.Size - i - 1;
			result[i] = GetOutput(index, cache);
		}
		return result;
	}

	private bool GetOutput(int index, Dictionary<int, bool> cache) {
		if (cache.ContainsKey(index)) return cache[index];
		var sourceIndexes = _data.GetInputsFor(index);
		var sourceValues = sourceIndexes.Select(i => GetOutput(i, cache));
		var output = !sourceValues.All(i => i); // NAND
		cache[index] = output;
		return output;
	}
}