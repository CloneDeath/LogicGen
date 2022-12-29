using Silk.NET.Vulkan;
using SilkNetConvenience;

namespace LogicGen.Compute;

public class ComputePipeline : IDisposable {
	public readonly Pipeline Pipeline;
	private readonly Device _device;
	private readonly Vk _vk;

	public ComputePipeline(Pipeline pipeline, Device device, Vk vk) {
		Pipeline = pipeline;
		_device = device;
		_vk = vk;
	}

	private void ReleaseUnmanagedResources() {
		_vk.DestroyPipeline(_device, Pipeline);
	}

	public void Dispose() {
		ReleaseUnmanagedResources();
		GC.SuppressFinalize(this);
	}

	~ComputePipeline() {
		ReleaseUnmanagedResources();
	}
}