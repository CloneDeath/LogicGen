namespace LogicGen.LogicGates; 

public class NOrGate : OrGate {
	public override string Name => "NOr";

	protected override bool ExecuteGate(params bool[] inputs) {
		return !base.ExecuteGate(inputs);
	}
}