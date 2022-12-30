using LogicGen.Compute.Shaders;

namespace LogicGen.Compute;

public interface IStage {
	public IShaderData Shader { get; }
	public BufferBinding[] Bindings { get; }
}

public class Stage : IStage {
	public IShaderData Shader { get; }
	public BufferBinding[] Bindings { get; set; } = Array.Empty<BufferBinding>();

	public Stage(IShaderData shader) {
		Shader = shader;
	}
}