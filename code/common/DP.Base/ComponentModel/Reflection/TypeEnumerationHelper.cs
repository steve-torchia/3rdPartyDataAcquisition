using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DP.Base.Reflection
{
    public static class TypeEnumerationHelper
    {
        public static List<Type> GetAllTypes(string probeDirectory, IEnumerable<string> excludeAssemblyPatterns, IEnumerable<string> excludeTypePatterns)
        {
            var assemblies = GetAssembliesInDirectory(probeDirectory, excludeAssemblyPatterns.ToArray());
            return GetAllTypes(assemblies, excludeTypePatterns);
        }

        public static List<Type> GetAllTypes(IEnumerable<Assembly> assemblies, IEnumerable<string> excludeTypePatterns)
        {
            return GetAllTypes(assemblies.ToArray(), excludeTypePatterns);
        }

        public static List<Type> GetAllTypes(Assembly[] assemblies, IEnumerable<string> excludeTypePatterns)
        {
            var types = new List<Type>();
            Array.ForEach(
                assemblies,
                a =>
                {
                    try
                    {
                        types.AddRange(a.GetTypes()
                            .Where(t => t.FullName == null || !excludeTypePatterns.Any(exclusion => t.FullName.ToLower().StartsWith(exclusion))));
                    }
                    catch (ReflectionTypeLoadException /*e*/)
                    {
                        //Logger.WarnFormat("Could not scan assembly: {0}. The reason is: {1}.", a.FullName, e.LoaderExceptions.First().Message, e);
                        return; //intentionally swallow exception
                    }
                });

            return types;
        }

        public static IEnumerable<Assembly> GetAssembliesInDirectory(string path, params string[] excludeAssemblyPatterns)
        {
            foreach (var a in GetAssembliesInDirectoryWithExtension(path, "*.exe", excludeAssemblyPatterns))
            {
                yield return a;
            }

            foreach (var a in GetAssembliesInDirectoryWithExtension(path, "*.dll", excludeAssemblyPatterns))
            {
                yield return a;
            }
        }

        public static IEnumerable<Assembly> GetAssembliesInDirectoryWithExtension(string path, string extension, params string[] excludeAssemblyPatterns)
        {
            var result = new List<Assembly>();

            foreach (FileInfo file in new DirectoryInfo(path).GetFiles(extension, SearchOption.AllDirectories))
            {
                try
                {
                    if (excludeAssemblyPatterns.Any(exclusion => file.Name.ToLower().StartsWith(exclusion)))
                    {
                        continue;
                    }

                    result.Add(Assembly.LoadFrom(file.FullName));
                }
                catch (BadImageFormatException /*bif*/)
                {
                    //if (bif.FileName.ToLower().Contains("system.data.sqlite.dll"))
                    //    throw new BadImageFormatException(
                    //        "You've installed the wrong version of System.Data.SQLite.dll on this machine. If this machine is x86, this dll should be roughly 800KB. If this machine is x64, this dll should be roughly 1MB. You can find the x86 file under /binaries and the x64 version under /binaries/x64. *If you're running the samples, a quick fix would be to copy the file from /binaries/x64 over the file in /binaries - you should 'clean' your solution and rebuild after.",
                    //        bif.FileName, bif);

                    //throw new InvalidOperationException(
                    //    "Could not load " + file.FullName +
                    //    ". Consider using 'Configure.With(AllAssemblies.Except(\"" + file.Name + "\"))' to tell NServiceBus not to load this file.",
                    //    bif);
                }
            }

            return result;
        }

        public static List<Type> GetAllSubclassTypes(IEnumerable<Assembly> assemblies, Type baseType)
        {
            var types = assemblies.ToList()
                .SelectMany(a => a.GetTypes())
                .Where(t => baseType.IsAssignableFrom(t));

            return types.ToList();
        }
    }
}
