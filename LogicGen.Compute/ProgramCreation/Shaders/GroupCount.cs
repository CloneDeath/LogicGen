namespace LogicGen.Compute.ProgramCreation.Shaders; 

public class GroupCount {
	public GroupCount(uint x = 1, uint y = 1, uint z = 1) {
		X = x;
		Y = y;
		Z = z;
	}
	public uint X { get; }
	public uint Y { get; }
	public uint Z { get; }
}