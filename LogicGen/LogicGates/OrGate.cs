using System.Linq;

namespace LogicGen.LogicGates; 

public class OrGate : Gate {
	protected override bool ExecuteGate(params bool[] inputs) {
		return inputs.Any(i => i);
	}
}