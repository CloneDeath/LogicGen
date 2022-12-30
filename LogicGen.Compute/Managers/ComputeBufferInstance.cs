using LogicGen.Compute.Components;
using LogicGen.Compute.ProgramCreation;
using SilkNetConvenience.Wrappers;

namespace LogicGen.Compute.Managers; 

public class ComputeBufferInstance : IDisposable {
	private readonly VulkanDeviceMemory _memory;
	private readonly VulkanBuffer _buffer;
	public ComputeBufferInstance(ComputeBuffer reference, ComputeDevice device) {
		_memory = device.AllocateMemory(reference.Size);
		_buffer = device.CreateBuffer(reference.Size);
		_buffer.BindMemory(_memory);
	}

	#region IDisposable
	private void ReleaseUnmanagedResources() {
		_buffer.Dispose();
		_memory.Dispose();
	}

	public void Dispose() {
		ReleaseUnmanagedResources();
		GC.SuppressFinalize(this);
	}

	~ComputeBufferInstance() {
		ReleaseUnmanagedResources();
	}
	#endregion
}