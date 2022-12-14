namespace LogicGen.ByteOperations; 

public class BitwiseOrOperation : ByteOperation {
	public override string Name => "BitwiseOr";
	protected override byte ExecuteOperation(byte a, byte b) {
		return (byte)(a | b);
	}
}