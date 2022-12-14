namespace LogicGen.ByteOperations; 

public class DivideOperation : ByteOperation {
	public override string Name => "Divide";
	protected override byte ExecuteOperation(byte a, byte b) {
		if (b == 0) return byte.MaxValue;
		return (byte)(a / b);
	}
}