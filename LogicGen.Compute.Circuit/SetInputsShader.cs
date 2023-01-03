using System.IO;
using LogicGen.Compute.ProgramCreation.Shaders;

namespace LogicGen.Compute.Circuit; 

public class SetInputsShader : IShaderData {
	private readonly int _width;
	public SetInputsShader(int width) {
		_width = width;
	}

	public byte[] Code => File.ReadAllBytes("Shader/set_inputs.spv");
	public string EntryPoint => "main";
	public IBufferDescription[] Buffers => new IBufferDescription[] {
		new BufferDescription(0, sizeof(int) + sizeof(int)*_width),
		new BufferDescription(1, sizeof(int) * _width)
	};
	public GroupCount Workers => new((uint)_width);
}