namespace EIS.Inventory.Shared.Helpers
{
    public static class StringExtensions
    {
        public static string Truncate(this string input, int max)
        {
            if (!string.IsNullOrEmpty(input) && input.Length > max)
                return string.Format("{0}...", input.Substring(0, max));

            return input;
        }

        public static string Right(this string str, int length)
        {
            //Check if the value is valid
            if (string.IsNullOrEmpty(str))
            {
                //Set valid empty string as string could be null
                str = string.Empty;
            }
            else if (str.Length > length)
            {
                //Make the string no longer than the max length
                str = str.Substring(str.Length - length, length);
            }

            //Return the string
            return str;
        }

        public static string RightStartAt(this string str, int startIndex)
        {
            //Check if the value is valid
            if (string.IsNullOrEmpty(str))
            {
                //Set valid empty string as string could be null
                str = string.Empty;
            }
            else if (str.Length > startIndex)
            {
                //Make the string no longer than the max length
                str = str.Substring(startIndex, str.Length - startIndex);
            }

            //Return the string
            return str;
        }
    }
}
