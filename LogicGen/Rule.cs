namespace LogicGen; 

public class Rule {
	public int Index { get; }
	public Matrix Values { get; }

	public Rule(int index, Matrix values) {
		Index = index;
		Values = values;
	}

	public override string ToString() {
		return $"{IndexToName.GetName(Index)} => {Values}";
	}

	public int this[int x, int y] => Values[x, y];
}