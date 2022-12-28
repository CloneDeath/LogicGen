using Silk.NET.Vulkan;
using SilkNetConvenience;
using SilkNetConvenience.CreateInfo;
using SilkNetConvenience.Exceptions;

namespace LogicGen.Compute; 

public static class Program {
	public static void Main() {
		var vk = Vk.GetApi();
		var instance = vk.CreateInstance(new InstanceCreateInformation {
			ApplicationInfo = new ApplicationInformation {
				ApplicationName = "Vulkan Compute Demo"
			},
			EnabledLayerNames = new[] {
				"VK_LAYER_KHRONOS_validation"
			}
		});

		var physicalDevices = vk.EnumeratePhysicalDevices(instance);
		var physicalDevice = PickPhysicalDevice(vk, physicalDevices);
		var queueFamilyIndex = GetComputeQueueFamilyIndex(vk, physicalDevice) ?? throw new Exception("Failed to find a queue family");
		var device = vk.CreateDevice(physicalDevice, new DeviceCreateInformation {
			QueueCreateInfos = new [] {
				new DeviceQueueCreateInformation {
					QueueFamilyIndex = queueFamilyIndex,
					QueuePriorities = new[]{1f}
				}
			}
		});

		var queue = vk.GetDeviceQueue(device, queueFamilyIndex, 0);
		const uint memorySize = 1024;
		var memoryTypeIndex = GetMemoryTypeIndex(vk, physicalDevice, (int)memorySize) 
		                      ?? throw new Exception("Could not find a suitable memory type");

		var deviceMemory = vk.AllocateMemory(device, new MemoryAllocateInformation {
			AllocationSize = memorySize,
			MemoryTypeIndex = (uint)memoryTypeIndex
		});

		const uint inBufferSize = 512;
		var inBuffer = vk.CreateBuffer(device, new BufferCreateInformation {
			Usage = BufferUsageFlags.StorageBufferBit,
			SharingMode = SharingMode.Exclusive,
			QueueFamilyIndices = new[] { queueFamilyIndex },
			Size = inBufferSize
		});
		var outBuffer = vk.CreateBuffer(device, new BufferCreateInformation {
			Usage = BufferUsageFlags.StorageBufferBit,
			SharingMode = SharingMode.Exclusive,
			QueueFamilyIndices = new []{ queueFamilyIndex },
			Size = memorySize - inBufferSize
		});
		vk.BindBufferMemory(device, inBuffer, deviceMemory, 0);
		vk.BindBufferMemory(device, outBuffer, deviceMemory, inBufferSize);

		var random = new Random();
		unsafe {
			int* data;
			vk.MapMemory(device, deviceMemory, 0, memorySize, 0, (void**)&data);
			for (var i = 0; i < memorySize / sizeof(int); i++) {
				data[i] = random.Next();
			}
			vk.UnmapMemory(device, deviceMemory);
		}

		var shaderModule = vk.CreateShaderModule(device, new ShaderModuleCreateInformation {
			Code = File.ReadAllBytes("Shader/sample.spv")
		});

		var layout = vk.CreateDescriptorSetLayout(device, new DescriptorSetLayoutCreateInformation {
			Bindings = new [] {
				new DescriptorSetLayoutBindingInformation {
					Binding = 0, 
					DescriptorType = DescriptorType.StorageBuffer,
					DescriptorCount = 1,
					StageFlags = ShaderStageFlags.ComputeBit
				},
				new DescriptorSetLayoutBindingInformation {
					Binding = 1,
					DescriptorType = DescriptorType.StorageBuffer,
					DescriptorCount = 1,
					StageFlags = ShaderStageFlags.ComputeBit
				}
			}
		});

		var pipelineLayout = vk.CreatePipelineLayout(device, new PipelineLayoutCreateInformation {
			SetLayouts = new [] {
				layout
			}
		});

		var pipeline = vk.CreateComputePipeline(device, default, new ComputePipelineCreateInformation {
			Layout = pipelineLayout,
			Stage = new PipelineShaderStateCreateInformation {
				Stage = ShaderStageFlags.ComputeBit,
				Module = shaderModule,
				Name = "main"
			}
		});

		var commandPool = vk.CreateCommandPool(device, new CommandPoolCreateInformation {
			QueueFamilyIndex = queueFamilyIndex
		});

		var commandBuffer = vk.AllocateCommandBuffers(device, new CommandBufferAllocateInformation {
			CommandPool = commandPool,
			CommandBufferCount = 1,
			Level = CommandBufferLevel.Primary
		}).First();

		vk.BeginCommandBuffer(commandBuffer, new CommandBufferBeginInfo {
			SType = StructureType.CommandBufferBeginInfo,
			Flags = CommandBufferUsageFlags.OneTimeSubmitBit
		}).AssertSuccess();
		
		// Continue here
		
		vk.EndCommandBuffer(commandBuffer).AssertSuccess();

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

	private static int? GetMemoryTypeIndex(Vk vk, PhysicalDevice physicalDevice, uint memorySize) {
		var memoryProperties = vk.GetPhysicalDeviceMemoryProperties(physicalDevice);
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
}