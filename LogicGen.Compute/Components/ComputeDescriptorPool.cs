using Silk.NET.Vulkan;
using SilkNetConvenience;
using SilkNetConvenience.CreateInfo;

namespace LogicGen.Compute.Components; 

public class ComputeDescriptorPool : IDisposable {
	private readonly DescriptorPool _descriptorPool;
	private readonly Device _device;
	private readonly Vk _vk;

	public ComputeDescriptorPool(uint descriptorCount, Device device, Vk vk) {
		_descriptorPool = vk.CreateDescriptorPool(device, new DescriptorPoolCreateInformation {
			PoolSizes = new [] {
				new DescriptorPoolSize {
					Type = DescriptorType.StorageBuffer,
					DescriptorCount = descriptorCount
				}
			},
			MaxSets = 1,
			Flags = DescriptorPoolCreateFlags.FreeDescriptorSetBit
		});
		
		_device = device;
		_vk = vk;
	}

	~ComputeDescriptorPool() {
		FreeUnmanagedResources();
	}

	public void Dispose() {
		FreeUnmanagedResources();
		GC.SuppressFinalize(this);
	}

	private void FreeUnmanagedResources() {
		_vk.DestroyDescriptorPool(_device, _descriptorPool);
	}

	public ComputeDescriptorSet AllocateDescriptorSet(ComputeDescriptorSetLayout descriptorSetLayout) {
		var descriptorSet = _vk.AllocateDescriptorSets(_device, new DescriptorSetAllocateInformation {
			SetLayouts = new[] { descriptorSetLayout.Layout },
			DescriptorPool = _descriptorPool
		}).First();
		return new ComputeDescriptorSet(descriptorSet, _descriptorPool, _device, _vk);
	}
}