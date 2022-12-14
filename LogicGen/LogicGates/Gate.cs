namespace LogicGen.LogicGates; 

public abstract class Gate : ITestCircuit {
	public bool[] Execute(params bool[] inputs) {
		return new[]{ExecuteGate(inputs)};
	}

	public abstract string Name { get; }
	protected abstract bool ExecuteGate(params bool[] inputs);
}