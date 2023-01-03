using LogicGen.Compute.Components;
using LogicGen.Compute.Managers;
using LogicGen.Compute.ProgramCreation;

namespace LogicGen.Compute; 

public class ComputeProgram : IDisposable {
	private readonly ComputeDevice _device;
	private readonly DescriptorManager _descriptorManager;
	private readonly BufferManager _bufferManager;
	
	private readonly ComputeCommandPool _commandPool;
	private readonly ComputeCommandBuffer _commandBuffer;
	private readonly List<IDisposable> _resources = new();

	public ComputeProgram(IStage[] stages) {
		_device = new ComputeDevice();

		var buffers = stages.SelectMany(s => s.Bindings.Select(b => b.Buffer))
			.GroupBy(b => b.Identifier).Select(bg => bg.First());
		_bufferManager = new BufferManager(_device, buffers);
		
		var descriptorSetCount = stages.Length;
		var storageBufferDescriptorCount = stages.Sum(s => s.Bindings.Length);
		_descriptorManager = new DescriptorManager(_device, (uint)descriptorSetCount, (uint)storageBufferDescriptorCount);
		
		_commandPool = _device.CreateCommandPool();
		_commandBuffer = _commandPool.AllocateCommandBuffer();
		_commandBuffer.Begin();
		
		foreach (var stage in stages) {
			var bindingIndices = stage.Bindings.Select(b => b.BindingIndex).ToArray();

			var descriptorSets = new List<DescriptorSetInfo>();
			foreach (var bufferBinding in stage.Bindings) {
				var bufferInstance = _bufferManager.GetBufferInstance(bufferBinding.Buffer);
				descriptorSets.Add(new DescriptorSetInfo(bufferBinding.BindingIndex, bufferInstance.Buffer));
			}

			using var descriptorSetLayout = _device.CreateDescriptorSetLayout(bindingIndices);
			var descriptorSet = _descriptorManager.AllocateDescriptorSet(descriptorSetLayout);
			descriptorSet.UpdateDescriptorSets(descriptorSets);

			var pipelineLayout = _device.CreatePipelineLayout(descriptorSetLayout);
			_resources.Add(pipelineLayout);

			var shaderModule = _device.CreateShaderModule(stage.Shader.Code);
			_resources.Add(shaderModule);
			var computePipeline = _device.CreateComputePipeline(pipelineLayout, shaderModule, stage.Shader.EntryPoint);
			_resources.Add(computePipeline);
			
			
			_commandBuffer.BindPipeline(computePipeline);
			_commandBuffer.BindDescriptorSet(pipelineLayout, descriptorSet);
			_commandBuffer.Dispatch(stage.Shader.Workers);
		}
		_commandBuffer.End();
	}

	public void Dispose() {
		_commandBuffer.Dispose();
		_commandPool.Dispose();

		foreach (var resource in ((IEnumerable<IDisposable>)_resources).Reverse()) {
			resource.Dispose();
		}

		_descriptorManager.Dispose();
		_bufferManager.Dispose();
		_device.Dispose();
		GC.SuppressFinalize(this);
	}

	public void Execute() {
		var queue = _device.GetDeviceQueue(0);
		queue.Submit(_commandBuffer);
		queue.WaitIdle();
	}

	public void Upload(ComputeBuffer computeBuffer, int[] data) => Upload(computeBuffer, data.SelectMany(BitConverter.GetBytes).ToArray());
	public void Upload(ComputeBuffer computeBuffer, byte[] data) {
		var bufferInstance = _bufferManager.GetBufferInstance(computeBuffer);
		var memory = bufferInstance.Memory;
		var memoryMap = memory.MapMemory();
		data.CopyTo(memoryMap);
		memory.UnmapMemory();
	}

	public unsafe T[] DownloadArray<T>(ComputeBuffer computeBuffer) where T : unmanaged {
		var data = Download(computeBuffer);
		var result = new T[data.Length / sizeof(T)];
		Buffer.BlockCopy(data, 0, result, 0, data.Length);
		return result;
	}
	public byte[] Download(ComputeBuffer computeBuffer) {
		var bufferInstance = _bufferManager.GetBufferInstance(computeBuffer);
		var memory = bufferInstance.Memory;
		var memoryMap = memory.MapMemory();
		var data = new byte[memoryMap.Length];
		memoryMap.CopyTo(data);
		memory.UnmapMemory();
		return data;
	}
}