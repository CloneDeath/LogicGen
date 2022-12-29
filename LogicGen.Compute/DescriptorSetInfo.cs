namespace LogicGen.Compute; 

public class DescriptorSetInfo {
	public uint BindingIndex { get; }
	public ComputeBuffer Buffer { get; }

	public DescriptorSetInfo(uint bindingIndex, ComputeBuffer buffer) {
		BindingIndex = bindingIndex;
		Buffer = buffer;
	}
}