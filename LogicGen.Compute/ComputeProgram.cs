using Silk.NET.Vulkan;
using SilkNetConvenience;
using SilkNetConvenience.CreateInfo;
using SilkNetConvenience.Exceptions;

namespace LogicGen.Compute; 

public class ComputeProgram {
	private readonly byte[] _code;
	private readonly string _entryPoint;

	public ComputeProgram(byte[] code, string entryPoint) {
		_code = code;
		_entryPoint = entryPoint;
	}
	public void Execute(InputData[] inputs, OutputData[] outputs) {
		using var vk = Vk.GetApi();
		var instance = vk.CreateInstance(new InstanceCreateInformation {
			ApplicationInfo = new ApplicationInformation {
				ApplicationName = "LogicGen.Compute"
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

		var memorySize = (uint)input.Length;
		var memoryTypeIndex = GetMemoryTypeIndex(vk, physicalDevice, memorySize) 
		                      ?? throw new Exception("Could not find a suitable memory type");

		var inputMemory = vk.AllocateMemory(device, new MemoryAllocateInformation {
			AllocationSize = memorySize,
			MemoryTypeIndex = (uint)memoryTypeIndex
		});

		var outputMemory = vk.AllocateMemory(device, new MemoryAllocateInformation {
			AllocationSize = (uint)input.Length,
			MemoryTypeIndex = (uint)memoryTypeIndex
		});

		var inBuffer = vk.CreateBuffer(device, new BufferCreateInformation {
			Usage = BufferUsageFlags.StorageBufferBit,
			SharingMode = SharingMode.Exclusive,
			QueueFamilyIndices = new[] { queueFamilyIndex },
			Size = (uint)input.Length
		});
		var outBuffer = vk.CreateBuffer(device, new BufferCreateInformation {
			Usage = BufferUsageFlags.StorageBufferBit,
			SharingMode = SharingMode.Exclusive,
			QueueFamilyIndices = new []{ queueFamilyIndex },
			Size = (uint)input.Length
		});
		vk.BindBufferMemory(device, inBuffer, inputMemory, 0);
		vk.BindBufferMemory(device, outBuffer, outputMemory, 0);

		unsafe {
			void* data = null;
			vk.MapMemory(device, inputMemory, 0, (uint)input.Length, 0, ref data);
			input.AsSpan().CopyTo(new Span<byte>(data, input.Length));
			vk.UnmapMemory(device, inputMemory);
		}

		var shaderModule = vk.CreateShaderModule(device, new ShaderModuleCreateInformation {
			Code = _code
		});

		var descriptorSetLayout = vk.CreateDescriptorSetLayout(device, new DescriptorSetLayoutCreateInformation {
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

		var descriptorPool = vk.CreateDescriptorPool(device, new DescriptorPoolCreateInformation {
			PoolSizes = new [] {
				new DescriptorPoolSize {
					Type = DescriptorType.StorageBuffer,
					DescriptorCount = 2
				}
			},
			MaxSets = 1
		});
		var descriptorSet = vk.AllocateDescriptorSets(device, new DescriptorSetAllocateInformation {
			SetLayouts = new[] { descriptorSetLayout },
			DescriptorPool = descriptorPool
		}).First();

		vk.UpdateDescriptorSets(device, new[] {
			new WriteDescriptorSetInfo {
				DstSet = descriptorSet,
				DstBinding = 0,
				DescriptorCount = 1,
				DescriptorType = DescriptorType.StorageBuffer,
				BufferInfo = new [] {
					new DescriptorBufferInfo {
						Buffer = inBuffer,
						Offset = 0,
						Range = Vk.WholeSize
					}
				}
			},
			new WriteDescriptorSetInfo {
				DstSet = descriptorSet,
				DstBinding = 1,
				DescriptorCount = 1,
				DescriptorType = DescriptorType.StorageBuffer,
				BufferInfo = new [] {
					new DescriptorBufferInfo {
						Buffer = outBuffer,
						Offset = 0,
						Range = Vk.WholeSize
					}
				}
			}
		}, Array.Empty<CopyDescriptorSetInfo>());

		var pipelineLayout = vk.CreatePipelineLayout(device, new PipelineLayoutCreateInformation {
			SetLayouts = new [] {
				descriptorSetLayout
			}
		});

		var computePipeline = vk.CreateComputePipeline(device, default, new ComputePipelineCreateInformation {
			Layout = pipelineLayout,
			Stage = new PipelineShaderStateCreateInformation {
				Stage = ShaderStageFlags.ComputeBit,
				Module = shaderModule,
				Name = _entryPoint
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
		
		vk.CmdBindPipeline(commandBuffer, PipelineBindPoint.Compute, computePipeline);
		vk.CmdBindDescriptorSets(commandBuffer, PipelineBindPoint.Compute, pipelineLayout, 0, 
			new[] { descriptorSet }, Array.Empty<uint>());
		vk.CmdDispatch(commandBuffer, (uint)input.Length, 1, 1);
		
		vk.EndCommandBuffer(commandBuffer).AssertSuccess();

		vk.QueueSubmit(queue, new SubmitInformation[] {
			new() {
				CommandBuffers = new[]{commandBuffer}
			}
		}, default);
		
		vk.QueueWaitIdle(queue).AssertSuccess();

		var result = new byte[input.Length];
		unsafe {
			void* output;
			vk.MapMemory(device, outputMemory, 0, (uint)result.Length, 0, &output);
			new Span<byte>(output, result.Length).CopyTo(result);
			vk.UnmapMemory(device, inputMemory);
		}
		return result;
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