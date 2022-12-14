using System;

namespace LogicGen; 

public static class IndexToName {
	public const int StartIndex = 16;

	public static string GetName(int index) {
		if (index == 0) return "0";
		if (index == 1) return "1";
		if (index < StartIndex) return GetCharOffset('a', index - 2);
		if (index - StartIndex < 26) return GetCharOffset('A', index - StartIndex);
		throw new NotImplementedException();
	}

	private static string GetCharOffset(char c, int offset) {
		return ((char)(c + (char)offset)).ToString();
	}
}