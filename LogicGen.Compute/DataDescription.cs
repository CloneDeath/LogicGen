namespace LogicGen.Compute;

public interface IDataDescription {
	public uint BindingIndex { get; }
	public uint Size { get; }
}

public class DataDescription : IDataDescription {
	public DataDescription(uint bindingIndex, uint size) {
		BindingIndex = bindingIndex;
		Size = size;
	}
	public uint BindingIndex { get; }
	public uint Size { get; }
}