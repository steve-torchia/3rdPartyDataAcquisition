using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;

namespace DP.Base
{
    public static class FileUtils
    {
        //Put some more stuff in here!! I'm lonely :(
        public static string GetFullFilePath(string fileName)
        {
            return Path.Combine(GetCurrentDirPath(), fileName);
        }

        public static string GetCurrentDirPath()
        {
            string codeBase = Assembly.GetExecutingAssembly().Location;
            UriBuilder uri = new UriBuilder(codeBase);
            return Path.GetDirectoryName(Uri.UnescapeDataString(uri.Path));
        }

        private static string[] suffixes = new[] { " B", " KB", " MB", " GB", " TB", " PB" };

        public static string ToSizeString(long number, int precision = 2)
        {
            const long unit = 1024;
            int i = 0;
            while (number > unit)
            {
                number /= unit;
                i++;
            }

            return System.Math.Round((double)number, precision) + suffixes[i];
        }
    }
}
