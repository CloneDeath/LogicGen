using Silk.NET.Vulkan;
using SilkNetConvenience.Exceptions;

namespace LogicGen.Compute.Components;

public class ComputeCommandBuffer : IDisposable {
	public readonly CommandBuffer CommandBuffer;
	private readonly CommandPool _commandPool;
	private readonly Device _device;
	private readonly Vk _vk;

	public ComputeCommandBuffer(CommandBuffer commandBuffer, CommandPool commandPool, Device device, Vk vk) {
		CommandBuffer = commandBuffer;
		_commandPool = commandPool;
		_device = device;
		_vk = vk;
	}
	
	private void ReleaseUnmanagedResources() {
		_vk.FreeCommandBuffers(_device, _commandPool, new[] { CommandBuffer });
	}

	public void Dispose() {
		ReleaseUnmanagedResources();
		GC.SuppressFinalize(this);
	}

	~ComputeCommandBuffer() {
		ReleaseUnmanagedResources();
	}

	public void Begin() {
		_vk.BeginCommandBuffer(CommandBuffer, new CommandBufferBeginInfo {
			SType = StructureType.CommandBufferBeginInfo,
			Flags = CommandBufferUsageFlags.OneTimeSubmitBit
		}).AssertSuccess();
	}

	public void End() {
		_vk.EndCommandBuffer(CommandBuffer).AssertSuccess();
	}

	public void BindPipeline(ComputePipeline computePipeline) {
		_vk.CmdBindPipeline(CommandBuffer, PipelineBindPoint.Compute, computePipeline.Pipeline);
	}

	public void BindDescriptorSet(ComputePipelineLayout pipelineLayout, ComputeDescriptorSet computeDescriptorSet) {
		_vk.CmdBindDescriptorSets(CommandBuffer, PipelineBindPoint.Compute, pipelineLayout.PipelineLayout,
			0, new[] { computeDescriptorSet.DescriptorSet }, Array.Empty<uint>());
	}

	public void Dispatch(GroupCount groupCount) {
		_vk.CmdDispatch(CommandBuffer, groupCount.X, groupCount.Y, groupCount.Z);
	}
}