namespace LogicGen; 

public class CircuitResult {
	public RuleSet RuleSet { get; }
	public ICircuit Circuit { get; }
	public double Error { get; }

	public CircuitResult(RuleSet ruleSet, ICircuit circuit, double error) {
		RuleSet = ruleSet;
		Circuit = circuit;
		Error = error;
	}
}