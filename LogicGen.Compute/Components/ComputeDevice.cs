using Silk.NET.Vulkan;
using SilkNetConvenience;
using SilkNetConvenience.CreateInfo;
using SilkNetConvenience.Wrappers;

namespace LogicGen.Compute.Components; 

public class ComputeDevice : IDisposable {
	private readonly Vk _vk;
	private readonly Instance _instance;
	private readonly PhysicalDevice _physicalDevice;
	private readonly uint _queueFamilyIndex;
	private readonly VulkanDevice _device;

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
		_device = new VulkanDevice(_physicalDevice, new DeviceCreateInformation {
			QueueCreateInfos = new [] {
				new DeviceQueueCreateInformation {
					QueueFamilyIndex = _queueFamilyIndex,
					QueuePriorities = new[]{1f}
				}
			}
		}, _vk);
	}

	#region IDisposable
	private void FreeUnmanagedResources() {
		_device.Dispose();
		_vk.DestroyInstance(_instance);
		_vk.Dispose();
	}

	~ComputeDevice() {
		FreeUnmanagedResources();
	}

	public void Dispose() {
		FreeUnmanagedResources();
		GC.SuppressFinalize(this);
	}
	#endregion

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
		return _device.AllocateMemory((uint)memoryTypeIndex, size);
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

	public VulkanBuffer CreateBuffer(ulong size) {
		return _device.CreateBuffer(new BufferCreateInformation {
			Usage = BufferUsageFlags.StorageBufferBit,
			SharingMode = SharingMode.Exclusive,
			QueueFamilyIndices = new[] { _queueFamilyIndex },
			Size = size
		});
	}

	public VulkanShaderModule CreateShaderModule(byte[] code) => _device.CreateShaderModule(code);

	public ComputeDescriptorPool CreateDescriptorPool(uint maxSets, uint descriptorCount) {
		return new ComputeDescriptorPool(maxSets, descriptorCount, _device.Device, _vk);
	}

	public ComputeDescriptorSetLayout CreateDescriptorSetLayout(IEnumerable<uint> bindingIndices) {
		var layout = _vk.CreateDescriptorSetLayout(_device.Device, new DescriptorSetLayoutCreateInformation {
			Bindings = bindingIndices.Select(i => new DescriptorSetLayoutBindingInformation {
				Binding = i,
				DescriptorType = DescriptorType.StorageBuffer,
				DescriptorCount = 1,
				StageFlags = ShaderStageFlags.ComputeBit
			}).ToArray()
		});
		return new ComputeDescriptorSetLayout(layout, _device.Device, _vk);
	}

	public ComputePipelineLayout CreatePipelineLayout(ComputeDescriptorSetLayout descriptorSetLayout) {
		var layout = _vk.CreatePipelineLayout(_device.Device, new PipelineLayoutCreateInformation {
			SetLayouts = new [] {
				descriptorSetLayout.Layout
			}
		});
		return new ComputePipelineLayout(layout, _device.Device, _vk);
	}

	public ComputePipeline CreateComputePipeline(ComputePipelineLayout pipelineLayout, VulkanShaderModule shaderModule, string entryPoint) {
		var computePipeline = _vk.CreateComputePipeline(_device.Device, default, new ComputePipelineCreateInformation {
			Layout = pipelineLayout.PipelineLayout,
			Stage = new PipelineShaderStateCreateInformation {
				Stage = ShaderStageFlags.ComputeBit,
				Module = shaderModule.ShaderModule,
				Name = entryPoint
			}
		});
		return new ComputePipeline(computePipeline, _device.Device, _vk);
	}

	public ComputeCommandPool CreateCommandPool() {
		var commandPool = _vk.CreateCommandPool(_device.Device, new CommandPoolCreateInformation {
			QueueFamilyIndex = _queueFamilyIndex
		});
		return new ComputeCommandPool(commandPool, _device.Device, _vk);
	}

	public ComputeQueue GetDeviceQueue(uint index) {
		var queue = _vk.GetDeviceQueue(_device.Device, _queueFamilyIndex, index);
		return new ComputeQueue(queue, _vk);
	}
}