namespace LogicGen.ByteOperations; 

public class AddOperation : ByteOperation {
	public override string Name => "Add";
	protected override byte ExecuteOperation(byte a, byte b) {
		return (byte)(a + b);
	}
}