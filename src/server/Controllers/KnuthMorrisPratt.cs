namespace server
{
    public static class KnuthMorrisPratt
    {
        public static int KMPSearch(string text, string pattern)
        {
            int n = text.Length;
            int m = pattern.Length;
            int[] b = ComputeBorder(pattern);
            int i = 0;
            int j = 0;

            while (i < n)
            {
                if (pattern[j] == text[i])
                {
                    if (j == m - 1)
                        return i - m + 1;
                    i++;
                    j++;
                }
                else if (j > 0)
                    j = b[j - 1];
                else
                    i++;
            }
            return -1;
        }

        public static int[] ComputeBorder(string pattern)
        {
            int m = pattern.Length;
            int[] b = new int[m];
            b[0] = 0;
            int j = 0;
            int i = 1;

            while (i < m)
            {
                if (pattern[i] == pattern[j])
                {
                    b[i] = j + 1;
                    i++;
                    j++;
                }
                else if (j > 0)
                    j = b[j - 1];
                else
                {
                    b[i] = 0;
                    i++;
                }
            }
            return b;
        }
    }
}