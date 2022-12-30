using LogicGen.Compute.Components;

namespace LogicGen.Compute.Managers; 

public class DescriptorManager : IDisposable {
	private readonly ComputeDevice _device;
	private readonly ComputeDescriptorPool _descriptorPool;
	private readonly List<ComputeDescriptorSet> _allocations = new();
	
	public DescriptorManager(ComputeDevice device, uint maxSets, uint descriptorCount) {
		_device = device;
		_descriptorPool = device.CreateDescriptorPool(maxSets, descriptorCount);
	}

	#region IDisposable
	private void ReleaseUnmanagedResources() {
		foreach (var allocation in _allocations) allocation.Dispose();
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

	public ComputeDescriptorSet AllocateDescriptorSet(ComputeDescriptorSetLayout descriptorSetLayout) {
		var descriptorSet = _descriptorPool.AllocateDescriptorSet(descriptorSetLayout);
		_allocations.Add(descriptorSet);
		return descriptorSet;
	}
}