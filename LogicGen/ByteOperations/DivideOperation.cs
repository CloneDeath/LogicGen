namespace LogicGen.ByteOperations; 

public class DivideOperation : ByteOperation {
	public override string Name => "Divide";
	protected override byte ExecuteOperation(byte a, byte b) {
		return (byte)(a / b);
	}
}