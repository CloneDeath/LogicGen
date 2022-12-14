namespace LogicGen.LogicGates; 

public class NAndGate : AndGate {
	protected override bool ExecuteGate(params bool[] inputs) {
		return !base.ExecuteGate(inputs);
	}
}