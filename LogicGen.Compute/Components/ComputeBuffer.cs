using Silk.NET.Vulkan;
using SilkNetConvenience;
using SilkNetConvenience.Wrappers;
using Buffer = Silk.NET.Vulkan.Buffer;

namespace LogicGen.Compute.Components; 

public class ComputeBuffer : IDisposable {
	public readonly Buffer Buffer;
	private readonly Device _device;
	private readonly Vk _vk;

	public ComputeBuffer(Buffer buffer, Device device, Vk vk) {
		Buffer = buffer;
		_device = device;
		_vk = vk;
	}

	#region IDisposable
	~ComputeBuffer() {
		FreeUnmanagedResources();
	}

	public void Dispose() {
		FreeUnmanagedResources();
		GC.SuppressFinalize(this);
	}

	private void FreeUnmanagedResources() {
		_vk.DestroyBuffer(_device, Buffer);
	}
	#endregion
	
	public void BindMemory(VulkanMemory memory) {
		_vk.BindBufferMemory(_device, Buffer, memory.Memory, 0);
	}
}