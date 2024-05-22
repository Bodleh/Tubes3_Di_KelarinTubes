namespace server{
    public static class BoyerMoore
    {
        public static bool BMSearch(string text, string pattern)
        {   
            if (pattern.Length > text.Length) {
                (pattern, text) = (text, pattern);
            }
            
            int[] last = BuildLast(pattern);
            int n = text.Length;
            int m = pattern.Length;
            int i = m - 1;

            if (i > n - 1)
            {
                return false;
            }

            int j = m - 1;
            do
            {
                if (pattern[j] == text[i])
                {
                    if (j == 0)
                    {
                        return true; 
                    }
                    else
                    {
                        i--;
                        j--;
                    }
                }
                else
                {
                    int lo = last[text[i]]; 
                    i = i + m - Math.Min(j, 1 + lo);
                    j = m - 1;
                }
            } while (i <= n - 1);

            return false; 
        }
        public static int[] BuildLast(string pattern)
        {
            int[] last = new int[pattern.Length];
            for (int i = 0; i < 128; i++)
            {
                last[i] = -1; 
            }
            for (int i = 0; i < pattern.Length; i++)
            {
                last[pattern[i]] = i;
            }
            return last;
        }
    }
}
