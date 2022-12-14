using System;

namespace LogicGen.LogicGates; 

public class NotGate : Gate {
	public override string Name => "Not";

	protected override bool ExecuteGate(params bool[] inputs) {
		if (inputs.Length > 1) throw new NotImplementedException();
		return !inputs[0];
	}
}