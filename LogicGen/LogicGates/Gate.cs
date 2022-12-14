namespace LogicGen.LogicGates; 

public abstract class Gate : ICircuit {
	public bool[] Execute(params bool[] inputs) {
		return new[]{ExecuteGate(inputs)};
	}

	protected abstract bool ExecuteGate(params bool[] inputs);
}