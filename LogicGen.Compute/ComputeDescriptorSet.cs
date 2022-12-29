using Silk.NET.Vulkan;
using SilkNetConvenience;
using SilkNetConvenience.CreateInfo;

namespace LogicGen.Compute; 

public class ComputeDescriptorSet : IDisposable {
	public readonly DescriptorSet DescriptorSet;
	private readonly DescriptorPool _pool;
	private readonly Device _device;
	private readonly Vk _vk;

	public ComputeDescriptorSet(DescriptorSet descriptorSet, DescriptorPool pool, Device device, Vk vk) {
		DescriptorSet = descriptorSet;
		_pool = pool;
		_device = device;
		_vk = vk;
	}

	~ComputeDescriptorSet() {
		ReleaseUnmanagedResources();
	}

	public void Dispose() {
		ReleaseUnmanagedResources();
		GC.SuppressFinalize(this);
	}

	private void ReleaseUnmanagedResources() {
		_vk.FreeDescriptorSets(_device, _pool, new[] { DescriptorSet });
	}

	public void UpdateDescriptorSets(IEnumerable<DescriptorSetInfo> descriptorSetInfos) {
		var writeSets = descriptorSetInfos.Select(info => new WriteDescriptorSetInfo {
			DstSet = DescriptorSet,
			DstBinding = info.BindingIndex,
			DescriptorCount = 1,
			DescriptorType = DescriptorType.StorageBuffer,
			BufferInfo = new [] {
				new DescriptorBufferInfo {
					Buffer = info.Buffer.Buffer,
					Offset = 0,
					Range = Vk.WholeSize
				}
			}
		}).ToArray();
		_vk.UpdateDescriptorSets(_device, writeSets, Array.Empty<CopyDescriptorSetInfo>());
	}
}