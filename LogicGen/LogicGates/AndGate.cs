using System.Linq;

namespace LogicGen.LogicGates; 

public class AndGate : Gate {
	public override string Name => "And";

	protected override bool ExecuteGate(params bool[] inputs) {
		return inputs.All(i => i);
	}
}