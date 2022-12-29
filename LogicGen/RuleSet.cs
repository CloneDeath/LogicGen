using System.Collections.Generic;
using System.Linq;

namespace LogicGen; 

public class RuleSet {
	public IReadOnlyList<Rule> VariableRules { get; }
	private static IReadOnlyList<Rule> ConstantRules { get; } = GetConstantRules().ToList();

	private IEnumerable<Rule> Rules => ConstantRules.Concat(VariableRules);

	public RuleSet(IEnumerable<Rule> rules) {
		VariableRules = rules.ToList();
	}
	
	public ICircuit GenerateCircuit(int inputs, int outputs, int intermediates) {
		var minSize = inputs + outputs + intermediates;
		var matrix = GetMatrix(new Matrix(new[,]{{IndexToName.StartIndex}}));
		while (matrix.Size < minSize) {
			matrix = GetMatrix(matrix);
		}

		matrix = matrix.ToBinary().ToForwardOnly();
		return new ComputeMatrixCircuit(inputs, outputs, matrix);
	}

	private Matrix GetMatrix(Matrix seed) {
		var output = new int[seed.Size * 2, seed.Size * 2];
		for (var sx = 0; sx < seed.Size; sx++) {
			for (var sy = 0; sy < seed.Size; sy++) {
				var sIndex = seed[sx, sy];
				var rule = GetRule(sIndex);

				for (var tx = 0; tx < 2; tx++) {
					for (var ty = 0; ty < 2; ty++) {
						var dx = sx * 2 + tx;
						var dy = sy * 2 + ty;

						output[dx, dy] = rule[tx, ty];
					}
				}
			}
		}
		return new Matrix(output);
	}

	protected Rule GetRule(int index) => Rules.First(r => r.Index == index);

	private static IEnumerable<Rule> GetConstantRules() {
		yield return new Rule(0, new Matrix(new[,] { { 0, 0 }, { 0, 0 } }));
		yield return new Rule(1, new Matrix(new[,] { { 1, 1 }, { 1, 1 } }));
		for (var i = 0; i < IndexToName.StartIndex - 2; i++) {
			var index = i + 2;
			var value = i + 1;
			var data = new[,] {
				{ (value >> 0) & 0x01, (value >> 1) & 0x01 },
				{ (value >> 2) & 0x01, (value >> 3) & 0x01 }
			};
			yield return new Rule(index, new Matrix(data));
		}
	}
}