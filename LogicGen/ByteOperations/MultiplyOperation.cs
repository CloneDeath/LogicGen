namespace LogicGen.ByteOperations; 

public class MultiplyOperation : ByteOperation {
	public override string Name => "Multiply";
	protected override byte ExecuteOperation(byte a, byte b) {
		return (byte)(a * b);
	}
}