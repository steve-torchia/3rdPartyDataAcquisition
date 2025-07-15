using System;
using System.Reflection;

namespace DP.Base.Utilities
{
    public delegate void AssemblyResolveEventHandler(object sender, AssemblyResolveEventArgs area);
    public class AssemblyResolveEventArgs : EventArgs
    {
        public ResolveEventArgs ResolveEventArgs { get; set; }
        public Assembly Assembly { get; set; }
    }
}
