namespace LogicGen.Compute;

public class BufferBinding {
	public uint BindingIndex { get; }
	public ComputeBuffer Buffer { get; }

	public BufferBinding(uint bindingIndex, ComputeBuffer buffer) {
		BindingIndex = bindingIndex;
		Buffer = buffer;
	}
}