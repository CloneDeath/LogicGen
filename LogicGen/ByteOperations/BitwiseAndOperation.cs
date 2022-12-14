namespace LogicGen.ByteOperations; 

public class BitwiseAndOperation : ByteOperation {
	public override string Name => "BitwiseAnd";
	protected override byte ExecuteOperation(byte a, byte b) {
		return (byte)(a & b);
	}
}