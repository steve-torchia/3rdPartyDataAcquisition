using DP.Base.Contracts.ComponentModel;

namespace DP.Base.Contracts
{
    public interface IContextContainer : IDisposableEx, IInitializeAfterCreate
    {
        IContext Context { get; set; }
    }

    public interface IObjectContextContainer : IContextContainer
    {
        new IObjectContext Context { get; set; }
    }
}