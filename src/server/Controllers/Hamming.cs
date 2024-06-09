namespace server
{
    public static class Hamming {
        public static int hammingDist(string text, string pattern) 
        { 
            int i = 0, count = 0; 
            while (i < text.Length) 
            { 
                if (text[i] != pattern[i]) 
                    count++; 
                i++; 
            }
            return count; 
        }  
    }
}