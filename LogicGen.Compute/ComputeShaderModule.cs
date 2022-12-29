using Silk.NET.Vulkan;
using SilkNetConvenience;

namespace LogicGen.Compute;

public class ComputeShaderModule : IDisposable {
	public readonly ShaderModule ShaderModule;
	private readonly Device _device;
	private readonly Vk _vk;

	public ComputeShaderModule(ShaderModule shaderModule, Device device, Vk vk) {
		ShaderModule = shaderModule;
		_device = device;
		_vk = vk;
	}

	private void ReleaseUnmanagedResources() {
		_vk.DestroyShaderModule(_device, ShaderModule);
	}

	public void Dispose() {
		ReleaseUnmanagedResources();
		GC.SuppressFinalize(this);
	}

	~ComputeShaderModule() {
		ReleaseUnmanagedResources();
	}
}