namespace LogicGen.Compute;

public interface IOutputData {
	public uint BindingIndex { get; }
	public int Size { get; }
	public byte[] Data { get; set; }
}

public class OutputData : IOutputData {
	public uint BindingIndex { get; set; }
	public int Size { get; set; }
	public byte[] Data  { get; set; } = Array.Empty<byte>();
}