using Silk.NET.Vulkan;
using SilkNetConvenience;

namespace LogicGen.Compute;

public class ComputeMemory : IDisposable {
	public DeviceMemory Memory { get; }
	public ulong Size { get; }
	private readonly Device _device;
	private readonly Vk _vk;

	public ComputeMemory(DeviceMemory memory, uint size, Device device, Vk vk) {
		Memory = memory;
		Size = size;
		_device = device;
		_vk = vk;
	}

	~ComputeMemory() {
		FreeUnmanagedResources();
	}
	
	public void Dispose() {
		FreeUnmanagedResources();
		GC.SuppressFinalize(this);
	}

	private void FreeUnmanagedResources() {
		_vk.FreeMemory(_device, Memory);
	}

	public Span<byte> MapMemory() {
		return _vk.MapMemory(_device, Memory, 0, Size);
	}
	
	public void UnmapMemory() {
		_vk.UnmapMemory(_device, Memory);
	}
}