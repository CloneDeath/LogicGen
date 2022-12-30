using Silk.NET.Vulkan;
using SilkNetConvenience;
using SilkNetConvenience.CreateInfo;
using SilkNetConvenience.Exceptions;

namespace LogicGen.Compute.Components;

public class ComputeQueue {
	private readonly Queue _queue;
	private readonly Vk _vk;

	public ComputeQueue(Queue queue, Vk vk) {
		_queue = queue;
		_vk = vk;
	}

	public void Submit(ComputeCommandBuffer commandBuffer) {
		_vk.QueueSubmit(_queue, new SubmitInformation[] {
			new() {
				CommandBuffers = new[]{commandBuffer.CommandBuffer}
			}
		}, default);
	}

	public void WaitIdle() {
		_vk.QueueWaitIdle(_queue).AssertSuccess();
	}
}