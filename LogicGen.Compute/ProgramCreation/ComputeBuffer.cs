namespace LogicGen.Compute.ProgramCreation; 

public class ComputeBuffer {
	public ulong Size { get; }
	public Guid Identifier { get; } = Guid.NewGuid();
	public ComputeBuffer(ulong size) {
		Size = size;
	}
}