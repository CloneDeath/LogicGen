namespace LogicGen.Compute; 

public class OutputData {
	public int BindingIndex { get; set; }
	public int Size { get; set; }
	public byte[] Data  { get; set; } = Array.Empty<byte>();
}