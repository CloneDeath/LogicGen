namespace LogicGen.LogicGates; 

public class NOrGate : OrGate {
	protected override bool ExecuteGate(params bool[] inputs) {
		return !base.ExecuteGate(inputs);
	}
}