using System.Linq;

namespace LogicGen.LogicGates; 

public class OrGate : Gate {
	public override string Name => "Or";

	protected override bool ExecuteGate(params bool[] inputs) {
		return inputs.Any(i => i);
	}
}