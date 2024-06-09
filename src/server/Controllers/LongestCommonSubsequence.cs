namespace server
{
    public static class LongestCommonSubsequence
    {
        public static int Compute(string text, string pattern)
        {
            int m = text.Length;
            int n = pattern.Length;
            int[] prev = new int[n + 1];
            int[] curr = new int[n + 1];

            for (int i = 1; i <= m; i++)
            {
                for (int j = 1; j <= n; j++)
                {
                    if (text[i - 1] == pattern[j - 1])
                    {
                        curr[j] = prev[j - 1] + 1;
                    }
                    else
                    {
                        curr[j] = Math.Max(prev[j], curr[j - 1]);
                    }
                }
                // Swap prev and curr
                var temp = prev;
                prev = curr;
                curr = temp;
            }

            return prev[n];
        }
    }
}
