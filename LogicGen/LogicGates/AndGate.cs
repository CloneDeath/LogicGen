using System.Linq;

namespace LogicGen.LogicGates; 

public class AndGate : Gate {
	protected override bool ExecuteGate(params bool[] inputs) {
		return inputs.All(i => i);
	}
}