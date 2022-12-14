using System.Linq;

namespace LogicGen.LogicGates; 

public class XOrGate : Gate {
	public override string Name => "XOr";

	protected override bool ExecuteGate(params bool[] inputs) {
		return inputs.Sum(i => i ? 1 : 0) % 2 == 1;
	}
}