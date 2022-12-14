namespace LogicGen.LogicGates; 

public class NAndGate : AndGate {
	public override string Name => "NAnd";

	protected override bool ExecuteGate(params bool[] inputs) {
		return !base.ExecuteGate(inputs);
	}
}