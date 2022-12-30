using Silk.NET.Vulkan;
using SilkNetConvenience;
using SilkNetConvenience.CreateInfo;
using SilkNetConvenience.Exceptions;
using SilkNetConvenience.Wrappers;

namespace LogicGen.Compute.Components; 

public class ComputeDevice : IDisposable {
	private readonly Vk _vk;
	private readonly Instance _instance;
	private readonly PhysicalDevice _physicalDevice;
	private readonly uint _queueFamilyIndex;
	private readonly Device _device;

	public ComputeDevice() {
		_vk = Vk.GetApi();
		_instance = _vk.CreateInstance(new InstanceCreateInformation {
			ApplicationInfo = new ApplicationInformation {
				ApplicationName = "LogicGen.Compute"
			},
			EnabledLayerNames = new[] {
				"VK_LAYER_KHRONOS_validation"
			}
		});

		var physicalDevices = _vk.EnumeratePhysicalDevices(_instance);
		_physicalDevice = PickPhysicalDevice(_vk, physicalDevices);
		_queueFamilyIndex = GetComputeQueueFamilyIndex(_vk, _physicalDevice) ?? throw new Exception("Failed to find a queue family");
		_device = _vk.CreateDevice(_physicalDevice, new DeviceCreateInformation {
			QueueCreateInfos = new [] {
				new DeviceQueueCreateInformation {
					QueueFamilyIndex = _queueFamilyIndex,
					QueuePriorities = new[]{1f}
				}
			}
		});
	}

	~ComputeDevice() {
		FreeUnmanagedResources();
	}

	public void Dispose() {
		FreeUnmanagedResources();
		GC.SuppressFinalize(this);
	}

	private void FreeUnmanagedResources() {
		_vk.DestroyDevice(_device);
		_vk.DestroyInstance(_instance);
		_vk.Dispose();
	}

	private static PhysicalDevice PickPhysicalDevice(Vk vk, IEnumerable<PhysicalDevice> physicalDevices) {
		foreach (var physicalDevice in physicalDevices) {
			var computeQueueFamilyIndex = GetComputeQueueFamilyIndex(vk, physicalDevice);
			if (computeQueueFamilyIndex != null) return physicalDevice;
		}

		throw new Exception("Could not find a suitable physical device.");
	}

	private static uint? GetComputeQueueFamilyIndex(Vk vk, PhysicalDevice physicalDevice) {
		var queueFamilyProperties = vk.GetPhysicalDeviceQueueFamilyProperties(physicalDevice);
		for (var index = 0; index < queueFamilyProperties.Length; index++) {
			var queueFamilyProperty = queueFamilyProperties[index];
			if (queueFamilyProperty.QueueFlags.HasFlag(QueueFlags.ComputeBit)) {
				return (uint)index;
			}
		}
		return null;
	}

	public VulkanDeviceMemory AllocateMemory(ulong size) {
		var memoryTypeIndex = GetMemoryTypeIndex(size) 
		                      ?? throw new Exception("Could not find a suitable memory type");

		return new VulkanDeviceMemory((uint)memoryTypeIndex, size, _device, _vk);
	}

	private int? GetMemoryTypeIndex(ulong memorySize) {
		var memoryProperties = _vk.GetPhysicalDeviceMemoryProperties(_physicalDevice);
		for (var index = 0; index < memoryProperties.MemoryTypeCount; index++) {
			var memoryType = memoryProperties.MemoryTypes[index];
			var memoryHeap = memoryProperties.MemoryHeaps[(int)memoryType.HeapIndex];
			var properties = memoryType.PropertyFlags;

			if (properties.HasFlag(MemoryPropertyFlags.HostVisibleBit)
			    && properties.HasFlag(MemoryPropertyFlags.HostCoherentBit)
			    && memorySize < memoryHeap.Size) {
				return index;
			}
		}
		return null;
	}

	public VulkanBuffer CreateBuffer(VulkanDeviceMemory deviceMemory) {
		return new VulkanBuffer(new BufferCreateInformation {
			Usage = BufferUsageFlags.StorageBufferBit,
			SharingMode = SharingMode.Exclusive,
			QueueFamilyIndices = new[] { _queueFamilyIndex },
			Size = deviceMemory.Size
		}, _device, _vk);
	}

	public ComputeShaderModule CreateShaderModule(byte[] code) {
		var shaderModule = _vk.CreateShaderModule(_device, new ShaderModuleCreateInformation {
			Code = code
		});
		return new ComputeShaderModule(shaderModule, _device, _vk);
	}

	public ComputeDescriptorPool CreateDescriptorPool(uint descriptorCount) {
		return new ComputeDescriptorPool(descriptorCount, _device, _vk);
	}

	public ComputeDescriptorSetLayout CreateDescriptorSetLayout(IEnumerable<uint> bindingIndices) {
		var bindings = bindingIndices.Select(i => new DescriptorSetLayoutBindingInformation {
			Binding = i,
			DescriptorType = DescriptorType.StorageBuffer,
			DescriptorCount = 1,
			StageFlags = ShaderStageFlags.ComputeBit
		}).ToArray();
		var layout = _vk.CreateDescriptorSetLayout(_device, new DescriptorSetLayoutCreateInformation {
			Bindings = bindings
		});
		return new ComputeDescriptorSetLayout(layout, _device, _vk);
	}

	public ComputePipelineLayout CreatePipelineLayout(ComputeDescriptorSetLayout descriptorSetLayout) {
		var layout = _vk.CreatePipelineLayout(_device, new PipelineLayoutCreateInformation {
			SetLayouts = new [] {
				descriptorSetLayout.Layout
			}
		});
		return new ComputePipelineLayout(layout, _device, _vk);
	}

	public ComputePipeline CreateComputePipeline(ComputePipelineLayout pipelineLayout, ComputeShaderModule shaderModule, string entryPoint) {
		var computePipeline = _vk.CreateComputePipeline(_device, default, new ComputePipelineCreateInformation {
			Layout = pipelineLayout.PipelineLayout,
			Stage = new PipelineShaderStateCreateInformation {
				Stage = ShaderStageFlags.ComputeBit,
				Module = shaderModule.ShaderModule,
				Name = entryPoint
			}
		});
		return new ComputePipeline(computePipeline, _device, _vk);
	}

	public ComputeCommandPool CreateCommandPool() {
		var commandPool = _vk.CreateCommandPool(_device, new CommandPoolCreateInformation {
			QueueFamilyIndex = _queueFamilyIndex
		});
		return new ComputeCommandPool(commandPool, _device, _vk);
	}

	public ComputeQueue GetDeviceQueue(uint index) {
		var queue = _vk.GetDeviceQueue(_device, _queueFamilyIndex, index);
		return new ComputeQueue(queue, _vk);
	}
}

public class ComputeQueue {
	private readonly Queue _queue;
	private readonly Vk _vk;

	public ComputeQueue(Queue queue, Vk vk) {
		_queue = queue;
		_vk = vk;
	}

	public void Submit(ComputeCommandBuffer commandBuffer) {
		_vk.QueueSubmit(_queue, new SubmitInformation[] {
			new() {
				CommandBuffers = new[]{commandBuffer.CommandBuffer}
			}
		}, default);
	}

	public void WaitIdle() {
		_vk.QueueWaitIdle(_queue).AssertSuccess();
	}
}