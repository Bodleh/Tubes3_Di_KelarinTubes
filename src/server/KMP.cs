public class KMP { 
    private int[] LPS(string pattern)
    {
        int patterLength = pattern.Length;
        int i = 1;
        int j = 0;

        int[] table = new int[patterLength];
        table[0] = 0;
 
        while (i < patterLength) {
            if (pattern[i] == pattern[j]) {
                j++;
                table[i] = j;
                i++;
            }
            else {
                if (j != 0) {
                    j = table[j - 1];
                }
                else {
                    table[i] = j;
                    i++;
                }
            }
        }
        return table;
    }
    
    // pattern must be shorter than text
    public bool KMPSearch(string pattern, string text)
    {
        int patternLength = pattern.Length;
        int textLength = text.Length;
 
        int j = 0; // index for pat
        int i = 0; // index for txt
 
        int[] table = LPS(pattern);
 
        while ((textLength - i) >= (patternLength - j)) {
            if (text[i] == pattern[j]) {
                i++;
                j++;
            }
            if (j == patternLength) {
                return true;
            }
 
            else if (i < textLength && pattern[j] != text[i]) {
                if (j != 0)
                    j = table[j - 1];
                else
                    i++;;
            }
        }
        return false;
    }
}
