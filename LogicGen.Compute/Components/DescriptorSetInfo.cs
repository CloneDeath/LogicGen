using SilkNetConvenience.Wrappers;

namespace LogicGen.Compute.Components; 

public class DescriptorSetInfo {
	public uint BindingIndex { get; }
	public VulkanBuffer Buffer { get; }

	public DescriptorSetInfo(uint bindingIndex, VulkanBuffer buffer) {
		BindingIndex = bindingIndex;
		Buffer = buffer;
	}
}