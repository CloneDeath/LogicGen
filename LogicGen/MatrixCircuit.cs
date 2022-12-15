using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace LogicGen; 

public class MatrixCircuit : ICircuit {
	private readonly int _inputs;
	private readonly int _outputs;
	private readonly Matrix _data;
	
	private readonly Dictionary<int, int[]> dependencyCache = new();

	public MatrixCircuit(int inputs, int outputs, Matrix data) {
		_inputs = inputs;
		_outputs = outputs;
		_data = data;
	}

	public bool[] Execute(params bool[] inputs) {
		if (_inputs != inputs.Length) throw new NotImplementedException();
		var cache = new Dictionary<int, bool>(_data.Size);
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
		if (cache.TryGetValue(index, out var co)) return co;

		int[] sourceIndexes;
		if (dependencyCache.TryGetValue(index, out var x)) {
			sourceIndexes = x;
		} else {
			sourceIndexes = _data.GetInputsFor(index);
			dependencyCache[index] = sourceIndexes;
		}
		var sourceValues = sourceIndexes.Select(i => GetOutput(i, cache));
		var output = !sourceValues.All(i => i); // NAND
		cache[index] = output;
		return output;
	}
}