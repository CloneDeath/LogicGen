namespace LogicGen; 

public interface ICircuit {
	public bool[] Execute(params bool[] inputs);
}