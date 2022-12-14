namespace LogicGen.LogicGates; 

public abstract class Gate : ITestCircuit {
	public bool[] Execute(params bool[] inputs) {
		return new[]{ExecuteGate(inputs)};
	}

	public abstract string Name { get; }
	public virtual int NumberOfInputs => 2;
	public virtual int NumberOfOutputs => 1;
	protected abstract bool ExecuteGate(params bool[] inputs);
}