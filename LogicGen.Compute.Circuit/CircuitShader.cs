using System.IO;
using LogicGen.Compute.ProgramCreation.Shaders;

namespace LogicGen.Compute.Circuit; 

public class CircuitShader : IShaderData {
	private readonly int _width;

	public CircuitShader(int width) {
		_width = width;
	}
	public byte[] Code => File.ReadAllBytes("Shader/circuit.spv");
	public string EntryPoint => "main";
	public IBufferDescription[] Buffers => new IBufferDescription[] {
		new BufferDescription(0, sizeof(int) + _width*_width*sizeof(int)),
		new BufferDescription(1, _width*sizeof(int)),
		new BufferDescription(2, _width*sizeof(int))
	};
	public GroupCount Workers => new((uint)_width);
}