using System.Collections.Concurrent;
using System.Collections.Generic;
using DP.Base.Contracts.ComponentModel;
using DP.Base.Contracts.Logging;
using DP.Base.Contracts.Security;
using DP.Base.Contracts.ServiceLocator;

namespace DP.Base.Contracts
{
    public interface IContext : IUserGroupInfoContainer, INamedComponent, IDisposableEx, IInitializeAfterCreate
    {
        void Initialize(IContext parentContext, IContextContainer container, UserGroupInformation userGroupInfo);

        ILogger Log { get; }

        ILoggerFactory LoggerFactory { get; }

        IContextContainer Container { get; }

        IServiceLocatorProvider ServiceLocatorProvider { get; set; }

        IServiceLocator ServiceLocator { get; }

        ConcurrentDictionary<string, object> PropertyBag { get; }

        DP.Base.Contracts.IDateTimeProvider DateTimeProvider { get; }

        IContext Parent { get; }

        IDiagnostic Diagnostic { get; }
    }

    public interface IGlobalContext : IContext
    {
    }

    public interface IObjectContext : IContext
    {
        IReflectiveMonitor ReflectiveMonitor { get; }

        //need some type ITransaction/unitOfWorkTransaction, maybe here or as a DAL parameter?
    }

    public interface IContextFinder
    {
        IContext GetCurrentContext();
    }

    public interface IObjectContextScope : IDisposableEx
    {
        /// <summary>
        /// Generally this will be automatically called in the ctor of the class
        /// </summary>
        //void PushContext(IObjectContext context);

        /// <summary>
        /// Generally this will be automatically called in the Dispose() of the class
        /// </summary>
        //void PopContext();
        IObjectContext GetThreadCurrentObjectContext();
    }
}
