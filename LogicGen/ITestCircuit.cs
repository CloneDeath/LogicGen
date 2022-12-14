namespace LogicGen; 

public interface ITestCircuit : ICircuit {
	public string Name { get; }
	
	public int NumberOfInputs { get; }
	public int NumberOfOutputs { get; }
}