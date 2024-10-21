namespace HenconExport
{
    public static class StringExtensions
    {
        public static string Clean(this string str)
        {
            return str.Replace("&nbsp;", string.Empty);
        }
    }
}
