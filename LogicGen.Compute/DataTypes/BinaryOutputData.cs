namespace LogicGen.Compute.DataTypes; 

public class BinaryOutputData : IOutputData {
	private readonly int _count;

	public BinaryOutputData(uint bindingIndex, int count) {
		BindingIndex = bindingIndex;
		_count = count;
		Data = new byte[count * sizeof(int)];
	}
	public uint BindingIndex { get; }
	public byte[] Data { get; set; }

	public bool[] BinaryData {
		get {
			var output = new bool[_count];
			for (var i = 0; i < _count; i++) {
				output[i] = BitConverter.ToInt32(Data, i * sizeof(int)) != 0;
			}
			return output;
		}
	}
}