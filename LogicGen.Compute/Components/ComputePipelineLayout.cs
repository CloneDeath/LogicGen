using Silk.NET.Vulkan;
using SilkNetConvenience;

namespace LogicGen.Compute.Components; 

public class ComputePipelineLayout : IDisposable {
	public readonly PipelineLayout PipelineLayout;
	private readonly Device _device;
	private readonly Vk _vk;

	public ComputePipelineLayout(PipelineLayout pipelineLayout, Device device, Vk vk) {
		PipelineLayout = pipelineLayout;
		_device = device;
		_vk = vk;
	}

	private void ReleaseUnmanagedResources() {
		_vk.DestroyPipelineLayout(_device, PipelineLayout);
	}

	public void Dispose() {
		ReleaseUnmanagedResources();
		GC.SuppressFinalize(this);
	}

	~ComputePipelineLayout() {
		ReleaseUnmanagedResources();
	}
}