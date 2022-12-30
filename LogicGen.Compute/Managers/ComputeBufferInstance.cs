using LogicGen.Compute.Components;
using LogicGen.Compute.ProgramCreation;
using SilkNetConvenience.Wrappers;

namespace LogicGen.Compute.Managers; 

public class ComputeBufferInstance : IDisposable {
	public Guid BufferIdentifier => _reference.Identifier;
	
	private readonly ComputeBuffer _reference;
	public readonly VulkanDeviceMemory Memory;
	public readonly VulkanBuffer Buffer;
	
	public ComputeBufferInstance(ComputeBuffer reference, ComputeDevice device) {
		_reference = reference;
		Memory = device.AllocateMemory(reference.Size);
		Buffer = device.CreateBuffer(reference.Size);
		Buffer.BindMemory(Memory);
	}

	#region IDisposable
	private void ReleaseUnmanagedResources() {
		Buffer.Dispose();
		Memory.Dispose();
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