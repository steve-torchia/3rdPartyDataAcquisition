using System;
using DP.Base.Contracts.Security;

namespace DP.Base.Contracts.ServiceLocator
{
    public delegate void GetInstanceEventHandler(IServiceLocator locator, GetInstanceEventArgs giea);

    public class GetInstanceEventArgs : System.EventArgs
    {
        public object Key { get; set; }

        public object NewInstance { get; private set; }
        
        public int NewInstanceAssignedCount { get; private set; }

        public void AssignNewInstance(object obj)
        {
            this.NewInstance = obj;
            this.NewInstanceAssignedCount++;
        }

        public Type RequestedType { get; set; }
    }

    public delegate void GetServiceLocatorEventHandler(IServiceLocatorProvider serviceLocatorProvider, GetServiceLocatorEventArgs gslea);

    public class GetServiceLocatorEventArgs : System.EventArgs
    {
        public IServiceLocator ServiceLocator { get; set; }

        public UserGroupInformation UserGroupInfo { get; set; }

        public IServiceLocator UserGroupParentServiceLocator { get; set; }
    }

    //public delegate void GetComponentRegistrarEventHandler(IComponentRegistrarProvider componentRegistrarProvider, 
    //                                                          GetComponentRegistrarEventArgs gcrea);

    //public class GetComponentRegistrarEventArgs : System.EventArgs
    //{
    //  public IComponentRegistrar ComponentRegistrar { get; set; }

    //  public ComponentRegistrationInfoCollection ComponentRegistrations
    //  {
    //      get;
    //      set;
    //  }

    //  public bool HasComponentRegistrations
    //  {
    //      get
    //      {
    //          return this.ComponentRegistrations != null && this.ComponentRegistrations.Components != null && this.ComponentRegistrations.Components.Any();
    //      }
    //  }

    //  public UserGroupInformation UserGroupInfo { get; set; }

    //}

    public delegate void ComponentRegistrationInfoEventHandler(IComponentRegistrationInfoProvider componentRegistrationInfoProvider,
                                                                GetComponentRegistrationInfosEventArgs gcriea);

    public class GetComponentRegistrationInfosEventArgs : System.EventArgs
    {
        public GetComponentRegistrationInfosEventArgs(UserGroupInformation userGroupInfo)
        {
            this.UserGroupInfo = userGroupInfo;
            this.ComponentRegistrationInfos = new ComponentRegistrationInfoCollection(userGroupInfo);
        }

        public ComponentRegistrationInfoCollection ComponentRegistrationInfos { get; set; }

        public UserGroupInformation UserGroupInfo { get; set; }
    }
}
