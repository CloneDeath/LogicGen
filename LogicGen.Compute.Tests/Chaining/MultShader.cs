using System.IO;
using LogicGen.Compute.ProgramCreation.Shaders;

namespace LogicGen.Compute.Tests.Chaining; 

public class MultShader : IShaderData {
	private readonly int _inputCount;

	public MultShader(int inputCount) {
		_inputCount = inputCount;
	}
	
	public byte[] Code => File.ReadAllBytes("Chaining/mult.spv");
	public string EntryPoint => "main";
	public IBufferDescription[] Buffers => new IBufferDescription[] {
		new BufferDescription(0, (uint)_inputCount * sizeof(int)),
		new BufferDescription(1, (uint)_inputCount * sizeof(int))
	};
	public GroupCount Workers => new((uint)_inputCount);
}