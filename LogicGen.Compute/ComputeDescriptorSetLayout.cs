using Silk.NET.Vulkan;
using SilkNetConvenience;

namespace LogicGen.Compute; 

public class ComputeDescriptorSetLayout : IDisposable {
	public DescriptorSetLayout Layout { get; }
	private readonly Device _device;
	private readonly Vk _vk;

	public ComputeDescriptorSetLayout(DescriptorSetLayout layout, Device device, Vk vk) {
		Layout = layout;
		_device = device;
		_vk = vk;
	}

	~ComputeDescriptorSetLayout() {
		ReleaseUnmanagedResources();
	}

	public void Dispose() {
		ReleaseUnmanagedResources();
		GC.SuppressFinalize(this);
	}

	private void ReleaseUnmanagedResources() {
		_vk.DestroyDescriptorSetLayout(_device, Layout);
	}
}