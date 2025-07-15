using System.Text;

namespace DP.Base.Utilities
{
    public static class StringUtilites
    {
        public static string MakeFixedWidth(string input, int width)
        {
            if (string.IsNullOrEmpty(input))
            {
                input = string.Empty;
            }

            int startIndex = System.Math.Max(0, input.Length - width);
            string fixedWidth = input.Substring(startIndex);
            return fixedWidth.PadRight(width);
        }

        public static string ToUpperStripNonAlphaNumeric(string text)
        {
            return text == null
                ? null
                : StripNonAlphaNumeric(text).ToUpperInvariant();
        }

        /// <summary>
        /// Return a string that is stripped of non-alphanumeric characters
        /// i.e. c.w => cw, c/w => cw, c w => cw
        /// </summary>
        /// <returns></returns>
        public static string StripNonAlphaNumeric(string text)
        {
            if (text == null)
            {
                return null;
            }

            var sb = new StringBuilder(text.Length);
            foreach (var symbolChar in text)
            {
                if (char.IsLetterOrDigit(symbolChar) == false)
                {
                    continue;
                }

                sb.Append(symbolChar);
            }

            return sb.ToString();
        }

        /// i.e. c.w => cw, c/w => cw, c w => cw
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string StripNonNumeric(string text)
        {
            if (text == null)
            {
                return null;
            }

            var sb = new StringBuilder(text.Length);
            foreach (var symbolChar in text)
            {
                if (char.IsDigit(symbolChar) == false)
                {
                    continue;
                }

                sb.Append(symbolChar);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Return a string that has all non-alphanumeric characters replaced
        /// i.e. c.w => c^w, c/w => c^w, c w => c^w
        /// </summary>
        /// <param name="text"></param>
        /// <param name="replacementChar"> </param>
        /// <returns></returns>
        public static string ReplaceNonAlphaNumeric(string text, string replacementText)
        {
            if (text == null)
            {
                return null;
            }

            //replace replacment with one character
            if (replacementText.Length > 1)
            {
                text = text.Replace(replacementText, "\x01");
            }

            var sb = new StringBuilder(text.Length);
            bool lastCharLetterOrDigit = true;
            foreach (var symbolChar in text)
            {
                if (char.IsLetterOrDigit(symbolChar) == false)
                {
                    if (lastCharLetterOrDigit == true) //only replace chars 1 time if there are a string of specials
                    {
                        sb.Append(replacementText);
                    }

                    lastCharLetterOrDigit = false;
                    continue;
                }

                lastCharLetterOrDigit = true;
                sb.Append(symbolChar);
            }

            return sb.ToString();
        }

        public static string FormatWithSign(double? d)
        {
            if (d == null)
            {
                return string.Empty;
            }

            var ret = d.Value.ToString("e");
            if (d.Value >= 0)
            {
                ret = "+" + ret;
            }

            return ret;
        }

        public static string FormatWithSign(decimal? d)
        {
            if (d == null)
            {
                return string.Empty;
            }

            var ret = d.Value.ToString("e");
            if (d.Value >= 0)
            {
                ret = "+" + ret;
            }

            return ret;
        }
    }
}
