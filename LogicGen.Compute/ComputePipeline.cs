using Silk.NET.Vulkan;
using SilkNetConvenience;

namespace LogicGen.Compute;

public class ComputePipeline : IDisposable {
	private readonly Pipeline _pipeline;
	private readonly Device _device;
	private readonly Vk _vk;

	public ComputePipeline(Pipeline pipeline, Device device, Vk vk) {
		_pipeline = pipeline;
		_device = device;
		_vk = vk;
	}

	private void ReleaseUnmanagedResources() {
		_vk.DestroyPipeline(_device, _pipeline);
	}

	public void Dispose() {
		ReleaseUnmanagedResources();
		GC.SuppressFinalize(this);
	}

	~ComputePipeline() {
		ReleaseUnmanagedResources();
	}
}