namespace LogicGen.Compute;

public interface IShaderData {
	public byte[] Code { get; }
	public string EntryPoint { get; }
}
public class ShaderData : IShaderData {
	public byte[] Code { get; set; } = Array.Empty<byte>();
	public string EntryPoint { get; set; } = string.Empty;
}