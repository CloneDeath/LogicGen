namespace LogicGen.Compute;

public interface IInputData {
	public uint BindingIndex { get; }
	public byte[] Data { get; }
}

public class InputData : IInputData {
	public uint BindingIndex { get; set; }
	public byte[] Data { get; set; } = Array.Empty<byte>();
}