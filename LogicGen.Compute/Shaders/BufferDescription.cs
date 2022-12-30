namespace LogicGen.Compute.Shaders;

public interface IBufferDescription {
	public uint BindingIndex { get; }
	public uint Size { get; }
}

public class BufferDescription : IBufferDescription {
	public BufferDescription(uint bindingIndex, uint size) {
		BindingIndex = bindingIndex;
		Size = size;
	}
	public uint BindingIndex { get; }
	public uint Size { get; }
}