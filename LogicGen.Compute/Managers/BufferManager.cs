using LogicGen.Compute.Components;
using LogicGen.Compute.ProgramCreation;

namespace LogicGen.Compute.Managers; 

public class BufferManager : IDisposable {
	private readonly ComputeDevice _device;
	private readonly IReadOnlyList<ComputeBufferInstance> _instances;

	public BufferManager(ComputeDevice device, IEnumerable<ComputeBuffer> buffers) {
		_device = device;
		_instances = buffers.Select(b => new ComputeBufferInstance(b, device)).ToList();
	}

	#region IDisposable
	private void ReleaseUnmanagedResources() {
		foreach (var instance in _instances) instance.Dispose();
	}

	public void Dispose() {
		ReleaseUnmanagedResources();
		GC.SuppressFinalize(this);
	}

	~BufferManager() {
		ReleaseUnmanagedResources();
	}
	#endregion

	public ComputeBufferInstance GetBufferInstance(ComputeBuffer buffer) {
		return _instances.First(i => i.BufferIdentifier == buffer.Identifier);
	}
}