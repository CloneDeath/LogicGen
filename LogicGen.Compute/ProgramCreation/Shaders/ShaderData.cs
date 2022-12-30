namespace LogicGen.Compute.ProgramCreation.Shaders;

public interface IShaderData {
	public byte[] Code { get; }
	public string EntryPoint { get; }
	public IBufferDescription[] Buffers { get; }
	public GroupCount Workers { get; }
}
public class ShaderData : IShaderData {
	public byte[] Code { get; set; } = Array.Empty<byte>();
	public string EntryPoint { get; set; } = string.Empty;
	public IBufferDescription[] Buffers { get; set; } = Array.Empty<IBufferDescription>();
	public GroupCount Workers { get; set; } = new();
}