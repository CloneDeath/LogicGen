namespace LogicGen.Compute; 

public class ComputeProgram {
	private readonly byte[] _code;
	private readonly string _entryPoint;

	public ComputeProgram(byte[] code, string entryPoint) {
		_code = code;
		_entryPoint = entryPoint;
	}
	public void Execute(InputData[] inputs, OutputData[] outputs, GroupCount workers) {
		using var device = new ComputeDevice();

		var descriptorSets = new List<DescriptorSetInfo>();

		var inputMemory = new List<ComputeMemory>();
		var inputBuffers = new List<ComputeBuffer>();
		foreach (var input in inputs) {
			var memory = device.AllocateMemory((uint)input.Data.Length);
			inputMemory.Add(memory);
			
			var buffer = device.CreateBuffer(memory);
			inputBuffers.Add(buffer);
			
			var data = memory.MapMemory();
			input.Data.CopyTo(data);
			memory.UnmapMemory();

			descriptorSets.Add(new DescriptorSetInfo(input.BindingIndex, buffer));
		}

		var outputMemory = new List<ComputeMemory>();
		var outputBuffers = new List<ComputeBuffer>();
		foreach (var output in outputs) {
			var memory = device.AllocateMemory((uint)output.Size);
			outputMemory.Add(memory);

			var buffer = device.CreateBuffer(memory);
			outputBuffers.Add(buffer);
			
			descriptorSets.Add(new DescriptorSetInfo(output.BindingIndex, buffer));
		}

		using var descriptorPool = device.CreateDescriptorPool((uint)(inputBuffers.Count + outputBuffers.Count));

		var inputBindings = inputs.Select(i => i.BindingIndex);
		var outputBindings = outputs.Select(o => o.BindingIndex);
		var bindingIndexes = inputBindings.Concat(outputBindings);

		using var descriptorSetLayout = device.CreateDescriptorSetLayout(bindingIndexes);

		using var descriptorSet = descriptorPool.AllocateDescriptorSet(descriptorSetLayout);
		descriptorSet.UpdateDescriptorSets(descriptorSets);

		using var pipelineLayout = device.CreatePipelineLayout(descriptorSetLayout);

		using var shaderModule = device.CreateShaderModule(_code);
		using var computePipeline = device.CreateComputePipeline(pipelineLayout, shaderModule, _entryPoint);

		using var commandPool = device.CreateCommandPool();
		using var commandBuffer = commandPool.AllocateCommandBuffer();
		commandBuffer.Begin();
		{
			commandBuffer.BindPipeline(computePipeline);
			commandBuffer.BindDescriptorSet(pipelineLayout, descriptorSet);
			commandBuffer.Dispatch(workers);
		}
		commandBuffer.End();

		var queue = device.GetDeviceQueue(0);
		queue.Submit(commandBuffer);
		queue.WaitIdle();

		for (var index = 0; index < outputs.Length; index++) {
			var output = outputs[index];
			var memory = outputMemory[index];

			output.Data = new byte[output.Size];
			var data = memory.MapMemory();
			data.CopyTo(output.Data);
			memory.UnmapMemory();
		}

		var disposables = new List<IDisposable>()
			.Concat(outputBuffers)
			.Concat(outputMemory)
			.Concat(inputBuffers)
			.Concat(inputMemory);
		foreach (var disposable in disposables) disposable.Dispose();
	}
}