namespace LogicGen.LogicGates; 

public abstract class Gate : ICircuit {
	public bool[] Execute(params bool[] inputs) {
		return new[]{ExecuteGate(inputs)};
	}

	public abstract string Name { get; }
	protected abstract bool ExecuteGate(params bool[] inputs);
}