using DP.Base.Contracts.ComponentModel;

namespace DP.Base.Contracts
{
    public interface IReflectiveMonitor : INamedComponent, ICategorizedComponent, IPropertyDescriptorProvider, IInitializeAfterCreate
    {
        bool? HasTargetBeenDisposed { get; }
        object Target { get; set; }
        long MonitorId { get; }
    }
}
