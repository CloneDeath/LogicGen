using System;

namespace LogicGen.LogicGates; 

public class NotGate : Gate {
	public override string Name => "Not";

	public override int NumberOfInputs => 1;

	protected override bool ExecuteGate(params bool[] inputs) {
		if (inputs.Length > 1) throw new NotImplementedException();
		return !inputs[0];
	}
}