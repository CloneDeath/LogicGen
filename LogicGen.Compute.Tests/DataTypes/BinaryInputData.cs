using System;
using System.Linq;

namespace LogicGen.Compute.Tests.DataTypes; 

public class BinaryInputData : IInputData {
	public BinaryInputData(uint bindingIndex, bool[] data) {
		BindingIndex = bindingIndex;
		Data = data.Select(d => d ? 1 : 0)
			.SelectMany(BitConverter.GetBytes)
			.ToArray();
	}
	
	public uint BindingIndex { get; }
	public byte[] Data { get; }
}