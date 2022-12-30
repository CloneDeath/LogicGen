namespace LogicGen.Compute.ProgramCreation; 

public class ComputeBuffer {
	public ulong Size { get; }
	public Guid Identifier { get; } = Guid.NewGuid();
	public ComputeBuffer(int size) : this((ulong)size){}
	public ComputeBuffer(ulong size) {
		Size = size;
	}
}