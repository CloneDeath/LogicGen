using LogicGen.Compute.ProgramCreation;

namespace LogicGen.Compute.Tests.Chaining; 

public class ChainingTest {
	[Test]
	public void AbleToChainMultipleShadersTogether() {
		var inputCount = 5;
		var addShader = new AddShader(inputCount);
		var multShader = new MultShader(inputCount);
		var a = new ComputeBuffer((ulong)inputCount * sizeof(int));
		var b = new ComputeBuffer((ulong)inputCount * sizeof(int));
		var c = new ComputeBuffer((ulong)inputCount * sizeof(int));
		var stages = new IStage[] {
			new Stage(addShader) {
				Bindings = new[] {
					new BufferBinding(0, a),
					new BufferBinding(1, b)
				}
			},
			new Stage(multShader) {
				Bindings = new[] {
					new BufferBinding(0, b),
					new BufferBinding(1, c)
				}
			}
		};
		var program = new ComputeProgram(stages);

		program.Upload(a, new[] { 1, 2, 3, 4, 5 });
		program.Execute();
		var results = program.Download(c);

		results.Should().ContainInOrder(new[] { 4, 6, 8, 10, 12 });
	}
}