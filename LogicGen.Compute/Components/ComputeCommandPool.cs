using Silk.NET.Vulkan;
using SilkNetConvenience;
using SilkNetConvenience.CreateInfo;

namespace LogicGen.Compute.Components;

public class ComputeCommandPool : IDisposable {
	private readonly CommandPool _commandPool;
	private readonly Device _device;
	private readonly Vk _vk;

	public ComputeCommandPool(CommandPool commandPool, Device device, Vk vk) {
		_commandPool = commandPool;
		_device = device;
		_vk = vk;
	}

	private void ReleaseUnmanagedResources() {
		_vk.DestroyCommandPool(_device, _commandPool);
	}

	public void Dispose() {
		ReleaseUnmanagedResources();
		GC.SuppressFinalize(this);
	}

	~ComputeCommandPool() {
		ReleaseUnmanagedResources();
	}

	public ComputeCommandBuffer AllocateCommandBuffer() {
		var commandBuffer = _vk.AllocateCommandBuffers(_device, new CommandBufferAllocateInformation {
			CommandPool = _commandPool,
			CommandBufferCount = 1,
			Level = CommandBufferLevel.Primary
		}).First();
		return new ComputeCommandBuffer(commandBuffer, _commandPool, _device, _vk);
	}
}