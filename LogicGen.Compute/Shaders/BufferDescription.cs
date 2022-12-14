namespace LogicGen.Compute.Shaders;

public interface IBufferDescription {
	public uint BindingIndex { get; }
	public ulong Size { get; }
}

public class BufferDescription : IBufferDescription {
	public BufferDescription(uint bindingIndex, ulong size) {
		BindingIndex = bindingIndex;
		Size = size;
	}
	public uint BindingIndex { get; }
	public ulong Size { get; }
}