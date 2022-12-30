using LogicGen.Compute.Components;

namespace LogicGen.Compute.Managers; 

public class DescriptorManager : IDisposable {
	private readonly ComputeDevice _device;
	private readonly ComputeDescriptorPool _descriptorPool;
	
	public DescriptorManager(ComputeDevice device, uint maxSets, uint descriptorCount) {
		_device = device;
		_descriptorPool = device.CreateDescriptorPool(maxSets, descriptorCount);
	}

	#region IDisposable
	private void ReleaseUnmanagedResources() {
		_descriptorPool.Dispose();
	}

	public void Dispose() {
		ReleaseUnmanagedResources();
		GC.SuppressFinalize(this);
	}

	~DescriptorManager() {
		ReleaseUnmanagedResources();
	}
	#endregion
}