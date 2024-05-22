public class BM {
	private int[] badChar(string pattern)
	{
        int patternLength = pattern.Length;
        int[] table = new int[128];
		int i;

		for (i = 0; i < 128; i++) {
			table[i] = -1;
        }

		for (i = 0; i < patternLength; i++) {
			table[pattern[i]] = i;
        }
        return table;
	}

    // pattern must be shorter than text
	public bool BMSearch(string pattern, string text)
	{
		int patternLength = pattern.Length;
		int textLength = text.Length;

		int[] table = badChar(pattern);

		int s = 0;
		while (s <= (textLength - patternLength)) {
			int j = patternLength - 1;

			while (j >= 0 && pattern[j] == text[s + j]) {
				j--;
            }

			if (j < 0) {
                return true;
			} else {
				s += Math.Max(1, j - table[text[s + j]]);
            }
		}
        return false;
	}
}