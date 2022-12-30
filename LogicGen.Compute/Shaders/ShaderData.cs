namespace LogicGen.Compute.Shaders;

public interface IShaderData {
	public byte[] Code { get; }
	public string EntryPoint { get; }
	public IBufferDescription[] Buffers { get; }
}
public class ShaderData : IShaderData {
	public byte[] Code { get; set; } = Array.Empty<byte>();
	public string EntryPoint { get; set; } = string.Empty;
	public IBufferDescription[] Buffers { get; set; } = Array.Empty<IBufferDescription>();
}