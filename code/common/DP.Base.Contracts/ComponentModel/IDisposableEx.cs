using System;

namespace DP.Base.Contracts.ComponentModel
{
    /// <summary>
    /// interface to allow component to report that it has been disposed
    /// </summary>
    public interface IDisposableEx : IDisposable
    {
        bool IsDisposed { get; }

        event EventHandler Disposing;
    }
}
