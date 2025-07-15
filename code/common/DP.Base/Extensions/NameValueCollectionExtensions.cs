using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Web;

namespace DP.Base.Extensions
{
    public static class NameValueCollectionExtensions
    {
        public static Dictionary<string, string> ToDictionary(this NameValueCollection nvc)
        {
            return nvc.AllKeys.ToDictionary(k => k, k => nvc[k]);
        }

        public static List<KeyValuePair<string, string>> ToList(this NameValueCollection nvc)
        {
            return nvc.AllKeys.Select(k => new KeyValuePair<string, string>(k, nvc[k])).ToList();
        }

        public static string ToQueryString(this NameValueCollection nvc)
        {
            if (nvc.Count == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            for (int i = 0; i < nvc.Count; i++)
            {
                string text = nvc.GetKey(i);

                text = HttpUtility.UrlEncode(text);

                string val = (text != null) ? (text + "=") : string.Empty;
                string[] vals = nvc.GetValues(i);

                if (sb.Length > 0)
                {
                    sb.Append('&');
                }

                if (vals == null || vals.Length == 0)
                {
                    sb.Append(val);
                }
                else
                {
                    if (vals.Length == 1)
                    {
                        sb.Append(val);
                        sb.Append(HttpUtility.UrlEncode(vals[0]));
                    }
                    else
                    {
                        for (int j = 0; j < vals.Length; j++)
                        {
                            if (j > 0)
                            {
                                sb.Append('&');
                            }

                            sb.Append(val);
                            sb.Append(HttpUtility.UrlEncode(vals[j]));
                        }
                    }
                }
            }

            return sb.ToString();
        }
    }
}