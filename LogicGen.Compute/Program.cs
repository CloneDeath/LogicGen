using Silk.NET.Vulkan;
using SilkNetConvenience;

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
					QueueFamilyIndex = (uint)queueFamilyIndex,
					QueuePriorities = new[]{1f}
				}
			}
		});

		var queue = vk.GetDeviceQueue(device, (uint)queueFamilyIndex, 0);
		const uint allocationSize = 100;
		var memoryTypeIndex = GetMemoryTypeIndex(vk, physicalDevice, (int)allocationSize) 
		                      ?? throw new Exception("Could not find a suitable memory type");

		var deviceMemory = vk.AllocateMemory(device, new MemoryAllocateInformation {
			AllocationSize = allocationSize,
			MemoryTypeIndex = (uint)memoryTypeIndex
		});
		vk.CreateBuffer()
		vk.BindBufferMemory()
	}

	private static PhysicalDevice PickPhysicalDevice(Vk vk, IEnumerable<PhysicalDevice> physicalDevices) {
		foreach (var physicalDevice in physicalDevices) {
			var computeQueueFamilyIndex = GetComputeQueueFamilyIndex(vk, physicalDevice);
			if (computeQueueFamilyIndex != null) return physicalDevice;
		}

		throw new Exception("Could not find a suitable physical device.");
	}

	private static int? GetComputeQueueFamilyIndex(Vk vk, PhysicalDevice physicalDevice) {
		var queueFamilyProperties = vk.GetPhysicalDeviceQueueFamilyProperties(physicalDevice);
		for (var index = 0; index < queueFamilyProperties.Length; index++) {
			var queueFamilyProperty = queueFamilyProperties[index];
			if (queueFamilyProperty.QueueFlags.HasFlag(QueueFlags.ComputeBit)) {
				return index;
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