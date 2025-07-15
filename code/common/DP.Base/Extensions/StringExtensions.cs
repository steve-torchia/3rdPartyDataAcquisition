using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Web;
using DP.Base.DateTimeUtilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DP.Base.Extensions
{
    public static class StringExtensions
    {
        public static bool Contains(this string s, string value, StringComparison comparisionType) => s == null ? value == null : s.IndexOf(value, comparisionType) >= 0;

        public static bool ContainsIgnoreCase(this string s, string value) => s.Contains(value, StringComparison.OrdinalIgnoreCase);

        public static bool ContainsIgnoreCase(this List<string> s, string value) => s.Contains(value, StringComparer.OrdinalIgnoreCase);

        public static string DefaultIfEmpty(this string s, string defaultValue) => string.IsNullOrWhiteSpace(s) ? defaultValue : s;

        public static string RemoveAllSpaces(this string s) => s.Replace(" ", string.Empty);

        public static bool EndsWithIgnoreCase(this string s, string value) => s == null ? value == null : s.EndsWith(value, StringComparison.OrdinalIgnoreCase);

        public static bool EqualsIgnoreCase(this string s, string value) => s == null ? value == null : s.Equals(value, StringComparison.OrdinalIgnoreCase);

        public static int IndexOfIgnoreCase(this string s, string value) => s.IndexOf(value, StringComparison.OrdinalIgnoreCase);

        public static bool IsNullOrEmpty(this string s) => string.IsNullOrEmpty(s);

        public static bool IsNullOrWhiteSpace(this string s) => string.IsNullOrWhiteSpace(s);

        public static bool StartsWithIgnoreCase(this string s, string value) => s == null ? value == null : s.StartsWith(value, StringComparison.OrdinalIgnoreCase);

        public static DateTime FromCompressedSortableDateTimePattern(this string s) => 
            DateTime.ParseExact(s, DateTimeConstants.DateTimePattern.CompressedSortableDateTime, CultureInfo.InvariantCulture);

        public static DateTime FromSortableYearPattern(this string s) =>
            DateTime.ParseExact(s, DateTimeConstants.DateTimePattern.SortableYear, CultureInfo.InvariantCulture);

        public static NameValueCollection FromQueryString(this string queryString) => queryString == null ? null : HttpUtility.ParseQueryString(queryString);

        public static NameValueCollection FromQueryStringSlim(this string queryString, bool decodeVariables = true, bool decodeInput = false)
        {
            if (string.IsNullOrWhiteSpace(queryString))
            {
                return new NameValueCollection();
            }

            if (decodeInput == true)
            {
                queryString = HttpUtility.UrlDecode(queryString);
            }

            var questionParts = queryString.Split(new char[] { '?' }, StringSplitOptions.RemoveEmptyEntries);
            var varPairs = questionParts[questionParts.Length - 1].Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
            var retValue = new NameValueCollection(varPairs.Length);

            foreach (var varPair in varPairs)
            {
                var equalsIndex = varPair.IndexOf('=');
                string[] pairParts = new string[2];

                if (equalsIndex == -1)
                {
                    pairParts[0] = varPair;
                    pairParts[1] = string.Empty;
                }
                else
                {
                    pairParts[0] = varPair.Substring(0, equalsIndex);

                    if (equalsIndex == varPair.Length - 1)
                    {
                        pairParts[1] = string.Empty;
                    }
                    else
                    {
                        pairParts[1] = varPair.Substring(equalsIndex + 1);
                    }
                }

                if (decodeVariables == true && pairParts.Length > 1)
                {
                    pairParts[1] = pairParts[1].UrlDecode();
                }

                retValue.Add(pairParts[0], (pairParts.Length == 1) ? string.Empty : pairParts[1]);
            }

            return retValue;
        }

        public static string RemapInternationalCharToAscii(char c)
        {
            string s = c.ToString().ToLowerInvariant();
            if ("àåáâäãåą".Contains(s))
            {
                return "a";
            }
            else if ("èéêëę".Contains(s))
            {
                return "e";
            }
            else if ("ìíîïı".Contains(s))
            {
                return "i";
            }
            else if ("òóôõöøőð".Contains(s))
            {
                return "o";
            }
            else if ("ùúûüŭů".Contains(s))
            {
                return "u";
            }
            else if ("çćčĉ".Contains(s))
            {
                return "c";
            }
            else if ("żźž".Contains(s))
            {
                return "z";
            }
            else if ("śşšŝ".Contains(s))
            {
                return "s";
            }
            else if ("ñń".Contains(s))
            {
                return "n";
            }
            else if ("ýÿ".Contains(s))
            {
                return "y";
            }
            else if ("ğĝ".Contains(s))
            {
                return "g";
            }
            else if (c == 'ř')
            {
                return "r";
            }
            else if (c == 'ł')
            {
                return "l";
            }
            else if (c == 'đ')
            {
                return "d";
            }
            else if (c == 'ß')
            {
                return "ss";
            }
            else if (c == 'Þ')
            {
                return "th";
            }
            else if (c == 'ĥ')
            {
                return "h";
            }
            else if (c == 'ĵ')
            {
                return "j";
            }
            else
            {
                return string.Empty;
            }
        }

        public static string FormatWithException(this string message, Exception e) => string.Format($"{message}{Environment.NewLine}{e}");

        public static string UrlDecode(this string url) => HttpUtility.UrlDecode(url);

        public static string UrlEncode(this string url) => HttpUtility.UrlEncode(url);

        //public static int GetReliableHashCode(this string stringToHash)
        //{
        //    unsafe
        //    {
        //        fixed (char* src = stringToHash)
        //        {
        //            int hash1 = 5381;
        //            int hash2 = hash1;

        //            int c;
        //            char* s = src;
        //            while ((c = s[0]) != 0)
        //            {
        //                hash1 = ((hash1 << 5) + hash1) ^ c;
        //                c = s[1];
        //                if (c == 0)
        //                {
        //                    break;
        //                }

        //                hash2 = ((hash2 << 5) + hash2) ^ c;
        //                s += 2;
        //            }

        //            return hash1 + (hash2 * 1566083941);
        //        }
        //    }
        //}

        // skt: not supported in netstandard 1.2, need 1.3 or greater
        //private static Regex _cashStasheRegex = new Regex(@"\$\{([a-z0-9]+)\}", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        ///// <summary>
        ///// Replaces ${token} styled replacement tokens with a model
        ///// </summary>
        ///// <param name="template"></param>
        ///// <param name="model"></param>
        ///// <returns></returns>
        //public static string FromCashStache(this string template, IDictionary<string, object> model)
        //{
        //    if (template.IsNullOrWhiteSpace() || !model.AnySafe()) return template;

        //    return _cashStasheRegex.Replace(template, evaluator =>
        //    {
        //        var token = evaluator.Groups[1].Value;

        //        object replacement;
        //        if (model.TryGetValue(token, out replacement))
        //        {
        //            return replacement.ToString();
        //        }

        //        return null;
        //    });
        //}

        //public static long GetReliableLongHashCode(this string stringToHash)
        //{
        //    unsafe
        //    {
        //        long hash1 = 5381;
        //        long hash2 = hash1;
        //        fixed (char* src = stringToHash)
        //        {
        //            long c;
        //            char* s = src;
        //            while ((c = s[0]) != 0)
        //            {
        //                hash1 = ((hash1 << 5) + hash1) ^ c;
        //                c = s[1];
        //                if (c == 0)
        //                {
        //                    break;
        //                }

        //                hash2 = ((hash2 << 5) + hash2) ^ c;
        //                s += 2;
        //            }
        //        }

        //        return hash1 + (hash2 * 1566083941);
        //    }
        //}

        //public static long GetReliableIntHashCode(this string stringToHash)
        //{
        //    unsafe
        //    {
        //        int hash1 = 5381;
        //        int hash2 = hash1;
        //        fixed (char* src = stringToHash)
        //        {
        //            int c;
        //            char* s = src;
        //            while ((c = s[0]) != 0)
        //            {
        //                hash1 = ((hash1 << 5) + hash1) ^ c;
        //                c = s[1];
        //                if (c == 0)
        //                {
        //                    break;
        //                }

        //                hash2 = ((hash2 << 5) + hash2) ^ c;
        //                s += 2;
        //            }
        //        }

        //        return hash1 + (hash2 * 1566083941);
        //    }
        //}

        public static List<int> ParseIntList(this string input)
        {
            var ret = new List<int>();
            if (string.IsNullOrWhiteSpace(input))
            {
                return ret;
            }

            var parts = input.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();

            foreach (var part in parts)
            {
                if (int.TryParse(part, out int val))
                {
                    ret.Add(val);
                }
                else
                {
                    throw new Exception($"Failed to parse integer value: '{part}'");
                }
            }

            return ret;
        }

        public static Guid? ParseGuidSafe(this string s)
        {
            return Guid.TryParse(s, out Guid g) ? (Guid?)g : null;
        }

        public static string Right(this string input, int count)
        {
            return input.Substring(System.Math.Max(input.Length - count, 0), System.Math.Min(count, input.Length));
        }

        public static bool IsValidJson(this string strInput)
        {
            if (string.IsNullOrWhiteSpace(strInput)) 
            { 
                return false; 
            }

            strInput = strInput.Trim();
            if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || //For object
                (strInput.StartsWith("[") && strInput.EndsWith("]"))) //For array
            {
                try
                {
                    var obj = JToken.Parse(strInput);
                    return true;
                }
                catch (JsonReaderException jex)
                {
                    //Exception in parsing json
                    Console.WriteLine(jex.Message);
                    return false;
                }
                catch (Exception ex) //some other exception
                {
                    Console.WriteLine(ex.ToString());
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public static string RemoveSpecialCharacters(this string str, bool ignoreSpaceCharacter = true)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if (ignoreSpaceCharacter && c == ' ')
                {
                    sb.Append(c);
                }
                else if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'))
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Remove some commonly-illegal characters in a file name/path
        /// </summary>
        /// <returns></returns>
        public static string RemoveIllegalFilePathCharacters(this string str)
        {
            string retVal;

            retVal =
                str
                .Replace("/", string.Empty)
                .Replace("\\", string.Empty)
                .Replace("%", string.Empty)
                .Replace("&", string.Empty)
                .Replace("$", string.Empty)
                .Replace("!", string.Empty)
                .Replace("`", string.Empty)
                .Replace("?", string.Empty)
                .Replace("#", string.Empty)
                .Replace("@", string.Empty)
                .Replace("|", string.Empty)
                .Replace("=", string.Empty)
                .Replace(":", string.Empty)
                .Replace("{", string.Empty)
                .Replace("}", string.Empty)
                .Replace("=", string.Empty)
                .Replace(">", string.Empty)
                .Replace("<", string.Empty);

            return retVal;
        }

        public static string RemoveCharacters(this string s, params char[] remove)
        {
            // splitting on the characters removes them, then rejoin
            // https://stackoverflow.com/questions/7411438/remove-characters-from-c-sharp-string
            if (s == null) return null;
            return string.Join(string.Empty, s.Split(remove));
        }

        public static SecureString ConvertToSecureString(this string password)
        {
            if (password == null)
                throw new ArgumentNullException("password");

            var securePassword = new SecureString();

            foreach (char c in password)
                securePassword.AppendChar(c);

            securePassword.MakeReadOnly();
            return securePassword;
        }

        /// <summary>
        /// Return a string value between two tokens in a string
        /// </summary>
        /// <param name="str"></param>
        /// <param name="startToken"></param>
        /// <param name="endToken"></param>
        /// <returns></returns>
        public static string SubStringBetweenTokens(this string str, string startToken, string endToken)
        {
            var startIdx = str.IndexOf(startToken);
            var endIdx = str.IndexOf(endToken);

            if (startIdx == -1 || endIdx == -1)
            {
                return string.Empty;
            }

            return str.Substring(startIdx + startToken.Length, endIdx - startIdx - startToken.Length);
        }

        public static MemoryStream ToMemoryStream(this string s)
        {
            // don't forget to dispose stream with "using":
            //      using (var stream = s.ToMemoryStream()) { ... }
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}
