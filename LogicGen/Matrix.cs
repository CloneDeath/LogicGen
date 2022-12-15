using System;
using System.Collections.Generic;

namespace LogicGen; 

public class Matrix {
	public int[,] Values { get; }
	public int Size => Values.GetLength(0);

	public Matrix(int[,] values) {
		if (values.GetLength(0) != values.GetLength(1)) throw new NotImplementedException();
		Values = values;
	}

	public int this[int x, int y] => Values[x, y];

	public override string ToString() {
		var width = Values.GetLength(0);
		var height = Values.GetLength(1);
		var chains = new List<string>();
		for (var y = 0; y < height; y++) {
			var chain = new List<string>();
			for (var x = 0; x < width; x++) {
				chain.Add(IndexToName.GetName(Values[y, x]));
			}
			chains.Add($"[{string.Join(", ", chain)}]");
		}
		return $"[{string.Join(", ", chains)}]";
	}

	public Matrix ToBinary() {
		var data = new int[Size, Size];
		for (var x = 0; x < Size; x++) {
			for (var y = 0; y < Size; y++) {
				var value = this[x, y];
				data[x, y] = value <= 1 ? value : 0;
			}
		}
		return new Matrix(data);
	}

	public Matrix ToForwardOnly() {
		var data = new int[Size, Size];
		for (var x = 0; x < Size; x++) {
			for (var y = 0; y < Size; y++) {
				var value = this[x, y];
				data[x, y] = y > x ? value : 0;
			}
		}
		return new Matrix(data);
	}

	private readonly Dictionary<int, int[]> dependencyCache = new();

	public int[] GetInputsFor(int index) {
		if (dependencyCache.ContainsKey(index)) return dependencyCache[index];
		
		var inputs = new List<int>();
		for (var x = 0; x < Size; x++) {
			if (this[x, index] == 1) inputs.Add(x);
		}
		var dependency = inputs.ToArray();
		dependencyCache[index] = dependency;
		return dependency;
	}
}