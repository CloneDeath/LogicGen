using System.Linq;

namespace LogicGen.LogicGates; 

public class XNOrGate : Gate {
	protected override bool ExecuteGate(params bool[] inputs) {
		return inputs.Sum(i => i ? 1 : 0) % 2 == 0;
	}
}