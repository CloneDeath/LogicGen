using System;

namespace LogicGen.ByteOperations; 

public abstract class ByteOperation : ITestCircuit {
	public abstract string Name { get; }
	public int NumberOfInputs => 16;
	public int NumberOfOutputs => 8;

	public bool[] Execute(params bool[] inputs) {
		if (inputs.Length != 16) throw new NotImplementedException();
		var a = ConvertBoolArrayToByte(inputs[..8]);
		var b = ConvertBoolArrayToByte(inputs[8..]);
		var result = ExecuteOperation(a, b);
		return ConvertByteToBoolArray(result);
	}

	protected abstract byte ExecuteOperation(byte a, byte b);

	// https://stackoverflow.com/questions/24322417/how-to-convert-bool-array-in-one-byte-and-later-convert-back-in-bool-array
	private static byte ConvertBoolArrayToByte(bool[] bits) {
		if (bits.Length != 8) throw new Exception("Got bits != 8");
		
		byte result = 0;
		var index = 0;

		foreach (var b in bits) {
			if (b) result |= (byte)(1 << (7 - index));
			index++;
		}
		return result;
	}
	
	private static bool[] ConvertByteToBoolArray(byte b) {
		var bits = new bool[8];
		for (var i = 0; i < 8; i++)
			bits[i] = (b & (1 << i)) != 0;

		Array.Reverse(bits);
		return bits;
	}
}