//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Text;
//using System.Threading;
//using DP.Base.Contracts.Security;
//using DP.Base.Contracts.ServiceLocator;
//using DP.Base.Utilities;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Serialization;

//namespace DP.Base.ServiceLocator
//{
//    public abstract class ServiceLocatorProvider : IServiceLocatorProvider
//    {
//        protected Dictionary<Guid, IServiceLocatorWrapper> rootServiceLocatorByGroupIdMap;
//        protected ReaderWriterLockSlim serviceLocatorByGroupIdMapRWL;

//        protected ServiceLocatorProvider()
//        {
//            this.rootServiceLocatorByGroupIdMap = new Dictionary<Guid, IServiceLocatorWrapper>();
//            this.serviceLocatorByGroupIdMapRWL = new ReaderWriterLockSlim();
//        }

//        public virtual void Initialize(IComponentRegistrationInfoProvider componentRegistrationInfoProvider)
//        {
//            if (componentRegistrationInfoProvider == null)
//            {
//                componentRegistrationInfoProvider = ApplicationEx.CreateClassFromConfig<IComponentRegistrationInfoProvider>(ApplicationEx.ConfigurationNames.DefaultComponentRegistrationInfoProvider);
//            }

//            this.ComponentRegistrationInfoProvider = componentRegistrationInfoProvider;
//        }

//        public IComponentRegistrationInfoProvider ComponentRegistrationInfoProvider
//        {
//            get;
//            protected set;
//        }

//        public IServiceLocator GetServiceLocator(UserGroupInformation userGroupInfo)
//        {
//            IServiceLocator retVal = null;

//            if (this.TryFindServiceLocatorCore(userGroupInfo, out retVal))
//            {
//                return retVal;
//            }

//            this.BuildServiceLocatorWrapper(userGroupInfo);

//            if (this.TryFindServiceLocatorCore(userGroupInfo, out retVal))
//            {
//                return retVal;
//            }
            
//            // E.A (06-04-2019): If we hit this point, something went wrong and were not able to get the service locator.
//            // Until we figure out the source of all the intermittent failures, we'll throw here so we can get stack trace info and print out other variables 
//            // that might be useful.
//            var errormsg = new StringBuilder();
//            errormsg.AppendLine("Intermittent Service locator error. retVal should not be null at this point!");
//            errormsg.AppendLine($"userGroupInfo: {userGroupInfo}");
//            errormsg.AppendLine($"rootServiceLocatorByGroupIdMap count: {this.rootServiceLocatorByGroupIdMap.Count}");
//            // Try go get the service locator again, just to check.
//            var success = this.TryFindServiceLocatorCore(userGroupInfo, out _);
//            errormsg.AppendLine($"Last attempt at getting the service locator. Success: {success}");

//            throw new ArgumentNullException(errormsg.ToString());
//        }

//        public void HandSerializationError(object sender, ErrorEventArgs errorArgs)
//        {
//            var currentError = errorArgs.ErrorContext.Error.Message;
//            errorArgs.ErrorContext.Handled = true;
//        }

//        protected virtual bool TryFindServiceLocatorCore(UserGroupInformation userGroupInfo, out IServiceLocator serviceLocator)
//        {
//            Guid userGroupId;
//            if (userGroupInfo == null)
//            {
//                userGroupId = Guid.Empty;
//            }
//            else
//            {
//                userGroupId = userGroupInfo.Id;
//            }

//            IServiceLocatorWrapper wrapper;
//            this.serviceLocatorByGroupIdMapRWL.EnterReadLock();
//            try
//            {
//                if (this.rootServiceLocatorByGroupIdMap.TryGetValue(userGroupId, out wrapper) == false)
//                {
//                    serviceLocator = null;
//                    return false;
//                }
//            }
//            finally
//            {
//                this.serviceLocatorByGroupIdMapRWL.ExitReadLock();
//            }

//            var localInitializeEvent = wrapper.InitializeEvent;
//            if (localInitializeEvent == null)
//            {
//                serviceLocator = wrapper.ServiceLocator;
//                return true;
//            }

//            if (localInitializeEvent.WaitOne(TimeSpan.FromSeconds(30)) == false)
//            {
//                var errorMsg = new StringBuilder();
//                errorMsg.AppendLine("E.A: Waited 30 FULL SECONDS, but localInitializeEvent was still not available");
//                errorMsg.AppendLine("E.A: THe hypothesis is that this leads to a service locator error. So we'll throw here. ");

//                throw new ApplicationException(errorMsg.ToString());
//            }

//            serviceLocator = wrapper.ServiceLocator;
//            return true;
//        }

//        protected abstract IServiceLocator CreateServiceLocator();

//        protected virtual void BuildServiceLocatorWrapper(UserGroupInformation userGroupInfo)
//        {
//            List<IServiceLocatorWrapper> createdServiceLocatorWrappers = new List<IServiceLocatorWrapper>();
//            this.serviceLocatorByGroupIdMapRWL.EnterWriteLock();
//            try
//            {
//                this.BuildServiceLocatorWrapperUnsafe(userGroupInfo, createdServiceLocatorWrappers);
//            }
//            finally
//            {
//                this.serviceLocatorByGroupIdMapRWL.ExitWriteLock();
//            }

//            List<ManualResetEvent> eventsToClose = new List<ManualResetEvent>();
//            foreach (var wrapper in createdServiceLocatorWrappers)
//            {
//                var localInitializeEvent = wrapper.InitializeEvent;
//                if (localInitializeEvent == null)
//                {
//                    continue;
//                }

//                IServiceLocator parentUserGroupServiceLocator = (wrapper.ParentUserGroupServiceLocatorWrapper == null) ? null : wrapper.ParentUserGroupServiceLocatorWrapper.ServiceLocator;

//                wrapper.ServiceLocator.Initialize(parentUserGroupServiceLocator,
//                                                    wrapper.UserGroupInfo,
//                                                    this.ComponentRegistrationInfoProvider);

//                localInitializeEvent.Set();
//                wrapper.InitializeEvent = null;
//                eventsToClose.Add(localInitializeEvent);
//            }

//            var t = System.Threading.Tasks.Task.Run(
//                async () =>
//                {
//                    await System.Threading.Tasks.Task.Delay(5000);
//                    foreach (var manualResetEvent in eventsToClose)
//                    {
//                        try
//                        {
//                            manualResetEvent.Close();
//                        }
//                        catch
//                        {
//                        }
//                    }
//                });
//        }

//        protected virtual IServiceLocatorWrapper BuildServiceLocatorWrapperUnsafe(UserGroupInformation userGroupInfo, List<IServiceLocatorWrapper> createdServiceLocatorWrappers)
//        {
//            IServiceLocatorWrapper currentWrapper = null;
//            //step back up to the first parentUserGroup locator we can find
//            Guid userGroupId;
//            if (userGroupInfo == null)
//            {
//                userGroupId = Guid.Empty;
//            }
//            else
//            {
//                userGroupId = userGroupInfo.Id;
//            }

//            if (this.rootServiceLocatorByGroupIdMap.TryGetValue(userGroupId, out currentWrapper))
//            {
//                return currentWrapper;
//            }

//            IServiceLocatorWrapper parentUserGroupServiceLocatorWrapper = null;
//            if (userGroupInfo != null)
//            {
//                parentUserGroupServiceLocatorWrapper = this.BuildServiceLocatorWrapperUnsafe(userGroupInfo.Parent, createdServiceLocatorWrappers);
//            }

//            var newLocator = this.CreateServiceLocator();
//            currentWrapper = new IServiceLocatorWrapper()
//            {
//                ParentUserGroupServiceLocatorWrapper = parentUserGroupServiceLocatorWrapper,
//                ServiceLocator = newLocator,
//                UserGroupInfo = userGroupInfo,
//            };

//            createdServiceLocatorWrappers.Add(currentWrapper);
//            this.rootServiceLocatorByGroupIdMap[userGroupId] = currentWrapper;

//            return currentWrapper;
//        }

//        public event GetServiceLocatorEventHandler GetServiceLocator_Post;

//        public event GetServiceLocatorEventHandler GetServiceLocator_Pre;

//        protected virtual void OnGetServiceLocator_Post(GetServiceLocatorEventArgs gslea)
//        {
//            if (this.GetServiceLocator_Post != null)
//            {
//                this.GetServiceLocator_Post(this, gslea);
//            }
//        }

//        protected virtual void OnGetServiceLocator_Pre(GetServiceLocatorEventArgs gslea)
//        {
//            if (this.GetServiceLocator_Pre != null)
//            {
//                this.GetServiceLocator_Pre(this, gslea);
//            }
//        }

//        protected class IServiceLocatorWrapper
//        {
//            public IServiceLocator ServiceLocator { get; set; }
//            public IServiceLocatorWrapper ParentUserGroupServiceLocatorWrapper { get; set; }
//            public UserGroupInformation UserGroupInfo { get; set; }

//            public ManualResetEvent InitializeEvent = new ManualResetEvent(false);
//        }

//        public IServiceLocator GetChildServiceLocator(UserGroupInformation userGroupInfo, IServiceLocator parentLocator)
//        {
//            if (parentLocator == null)
//            {
//                return this.GetServiceLocator(userGroupInfo);
//            }

//            if (parentLocator.UserGroupInfo == null && userGroupInfo == null)
//            {
//                return parentLocator.CreateSubScope();
//            }

//            if (parentLocator.UserGroupInfo == null)
//            {
//                return this.GetServiceLocator(userGroupInfo);
//            }

//            if (userGroupInfo == null)
//            {
//                return this.GetServiceLocator(userGroupInfo);
//            }

//            if (parentLocator.UserGroupInfo.Id == userGroupInfo.Id)
//            {
//                return parentLocator.CreateSubScope();
//            }

//            return this.GetServiceLocator(userGroupInfo);
//        }
//    }
//}