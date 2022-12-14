namespace LogicGen.ByteOperations; 

public class SubtractOperation : ByteOperation {
	public override string Name => "Subtract";
	protected override byte ExecuteOperation(byte a, byte b) {
		return (byte)(a - b);
	}
}