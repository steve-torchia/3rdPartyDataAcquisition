using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DP.Base.Test
{
    public class Utils
    {
        public static string FindBuildTreeRoot(string startDir = ".")
        {
            var rootDirMarker = "rootOfBuildTree.txt";

            // find root of build tree
            var rootDir = string.Empty;
            var done = false;
            var curDir = new DirectoryInfo(startDir);
            while (curDir.Parent != null && !done)
            {
                if (File.Exists(Path.Combine(curDir.FullName, rootDirMarker)))
                {
                    rootDir = curDir.FullName;
                    break;
                }
                else
                {
                    curDir = curDir.Parent;
                }
            }
            if (string.IsNullOrEmpty(rootDir))
            {
                throw new Exception($"Could not find solution root from sourceDir='{Path.GetFullPath(startDir)}'");
            }

            return rootDir;
        }
    }
}
