namespace helpers
{
    class helpers
    {
        public static bool matchesAt(string source, int index, string search)
        {
            int i = 0;
            foreach (char c in search){
                if (c != source[index + i++]) {
                    return false;
                }
            }
            return true;
        }
    }
}