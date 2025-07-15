namespace DP.Base.Contracts
{
    public interface IServiceGetter
    {
        T GetServiceInstance<T>();
    }
}
