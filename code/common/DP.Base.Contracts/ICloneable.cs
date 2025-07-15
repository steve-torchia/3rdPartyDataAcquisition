using System;

namespace DP.Base.Contracts
{
    public interface ICloneable<T> : ICloneable where T : class, ICloneable
    {
        new T Clone();
    }
}
