using Silk.NET.Vulkan;
using SilkNetConvenience.CreateInfo;

namespace LogicGen.Compute; 

public class ComputeProgram {
	private readonly byte[] _code;
	private readonly string _entryPoint;

	public ComputeProgram(byte[] code, string entryPoint) {
		_code = code;
		_entryPoint = entryPoint;
	}
	public void Execute(InputData[] inputs, OutputData[] outputs) {
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

		var commandPool = vk.CreateCommandPool(device, new CommandPoolCreateInformation {
			QueueFamilyIndex = queueFamilyIndex
		});

		var commandBuffer = vk.AllocateCommandBuffers(device, new CommandBufferAllocateInformation {
			CommandPool = commandPool,
			CommandBufferCount = 1,
			Level = CommandBufferLevel.Primary
		}).First();

		vk.BeginCommandBuffer(commandBuffer, new CommandBufferBeginInfo {
			SType = StructureType.CommandBufferBeginInfo,
			Flags = CommandBufferUsageFlags.OneTimeSubmitBit
		}).AssertSuccess();
		
		vk.CmdBindPipeline(commandBuffer, PipelineBindPoint.Compute, computePipeline);
		vk.CmdBindDescriptorSets(commandBuffer, PipelineBindPoint.Compute, pipelineLayout, 0, 
			new[] { descriptorSet }, Array.Empty<uint>());
		vk.CmdDispatch(commandBuffer, (uint)input.Length, 1, 1);
		
		vk.EndCommandBuffer(commandBuffer).AssertSuccess();

		
		var queue = vk.GetDeviceQueue(device, queueFamilyIndex, 0);
		vk.QueueSubmit(queue, new SubmitInformation[] {
			new() {
				CommandBuffers = new[]{commandBuffer}
			}
		}, default);
		
		vk.QueueWaitIdle(queue).AssertSuccess();

		var result = new byte[input.Length];
		unsafe {
			void* output;
			vk.MapMemory(device, outputMemory, 0, (uint)result.Length, 0, &output);
			new Span<byte>(output, result.Length).CopyTo(result);
			vk.UnmapMemory(device, inputMemory);
		}

		var disposables = new List<IDisposable>()
			.Concat(outputBuffers)
			.Concat(outputMemory)
			.Concat(inputBuffers)
			.Concat(inputMemory);

		foreach (var disposable in disposables) disposable.Dispose();

		return result;
	}
}