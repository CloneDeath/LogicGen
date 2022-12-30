using LogicGen.Compute.Components;
using LogicGen.Compute.Managers;
using LogicGen.Compute.ProgramCreation;
using SilkNetConvenience.Wrappers;

namespace LogicGen.Compute; 

public class ComputeProgram : IDisposable {
	private readonly ComputeDevice _device;
	private readonly DescriptorManager _descriptorManager;
	private readonly BufferManager _bufferManager;
	
	private readonly ComputeDescriptorSetLayout _descriptorSetLayout;
	private readonly List<VulkanDeviceMemory> _memory = new();
	private readonly List<VulkanBuffer> _buffers = new();
	private readonly ComputeDescriptorSet _descriptorSet;
	private readonly ComputeShaderModule _shaderModule;
	private readonly ComputePipeline _computePipeline;
	private readonly ComputeCommandPool _commandPool;
	private readonly ComputeCommandBuffer _commandBuffer;

	private readonly Dictionary<uint, VulkanDeviceMemory> _bindingMemoryMap = new();

	public ComputeProgram(IStage[] stages) {
		_device = new ComputeDevice();

		var buffers = stages.SelectMany(s => s.Bindings.Select(b => b.Buffer));
		_bufferManager = new BufferManager();

		var descriptorSetCount = stages.Length;
		var storageBufferDescriptorCount = stages.Sum(s => s.Bindings.Length);
		_descriptorManager = new DescriptorManager(_device, (uint)descriptorSetCount, (uint)storageBufferDescriptorCount);
		
		var descriptorSets = new List<DescriptorSetInfo>();
		foreach (var dataDescription in shaderData.Buffers) {
			var memory = _device.AllocateMemory(dataDescription.Size);
			_memory.Add(memory);
			_bindingMemoryMap[dataDescription.BindingIndex] = memory;
			
			var buffer = _device.CreateBuffer(memory);
			buffer.BindMemory(memory);
			_buffers.Add(buffer);

			descriptorSets.Add(new DescriptorSetInfo(dataDescription.BindingIndex, buffer));
		}

		var bindingIndices = shaderData.Buffers.Select(d => d.BindingIndex).ToArray();
		_descriptorSetLayout = _device.CreateDescriptorSetLayout(bindingIndices);
		
		_descriptorSet = _descriptorPool.AllocateDescriptorSet(_descriptorSetLayout);
		_descriptorSet.UpdateDescriptorSets(descriptorSets);

		using var pipelineLayout = _device.CreatePipelineLayout(_descriptorSetLayout);

		_shaderModule = _device.CreateShaderModule(shaderData.Code);
		_computePipeline = _device.CreateComputePipeline(pipelineLayout, _shaderModule, shaderData.EntryPoint);

		_commandPool = _device.CreateCommandPool();
		_commandBuffer = _commandPool.AllocateCommandBuffer();
		_commandBuffer.Begin();
		{
			_commandBuffer.BindPipeline(_computePipeline);
			_commandBuffer.BindDescriptorSet(pipelineLayout, _descriptorSet);
			_commandBuffer.Dispatch(shaderData.Workers);
		}
		_commandBuffer.End();
	}

	public void Dispose() {
		_commandBuffer.Dispose();
		_commandPool.Dispose();
		_computePipeline.Dispose();
		_shaderModule.Dispose();
		_descriptorSet.Dispose();
		
		var disposables = new List<IDisposable>()
			.Concat(_memory)
			.Concat(_buffers);
		foreach (var disposable in disposables) disposable.Dispose();
		
		_descriptorSetLayout.Dispose();
		_descriptorManager.Dispose();
		_bufferManager.Dispose();
		_device.Dispose();
		GC.SuppressFinalize(this);
	}

	public void Execute(IInputData[] inputs, IOutputData[] outputs) {
		foreach (var input in inputs) {
			var memory = _bindingMemoryMap[input.BindingIndex];
			var data = memory.MapMemory();
			input.Data.CopyTo(data);
			memory.UnmapMemory();
		}
		
		var queue = _device.GetDeviceQueue(0);
		queue.Submit(_commandBuffer);
		queue.WaitIdle();

		foreach (var output in outputs) {
			var memory = _bindingMemoryMap[output.BindingIndex];
			var data = memory.MapMemory();
			output.Data = new byte[data.Length];
			data.CopyTo(output.Data);
			memory.UnmapMemory();
		}
	}
}