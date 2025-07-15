using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DP.Base.Contracts.Security;

namespace DP.Base.Contracts.ServiceLocator
{
    public class ComponentRegistrationInfo
    {
        public ComponentRegistrationInfo()
        {
        }

        public ComponentRegistrationInfo(ComponentRegistrationInfo copy)
        {
            this.DisableLifeTimeManagement = copy.DisableLifeTimeManagement;
            this.implementationInstance = copy.implementationInstance;
            this.implementationType = copy.implementationType;
            this.implementationTypeName = copy.implementationTypeName;

            this.InjectProperties = copy.InjectProperties;
            this.SharedInstanceType = copy.SharedInstanceType;

            this.UserGroupId = copy.UserGroupId;

            if (copy.ConstructorParameterValues != null)
            {
                this.ConstructorParameterValues = copy.ConstructorParameterValues.Select(a => new PropertyValue(a)).ToList();
            }

            if (copy.PropertyValues != null)
            {
                this.PropertyValues = copy.PropertyValues.Select(a => new PropertyValue(a)).ToList();
            }

            if (copy.ServiceExports != null)
            {
                this.ServiceExports = copy.ServiceExports.Select(a => new ExportInformation(a)).ToList();
            }

            this.Ordinal = copy.Ordinal;

            this.CommonTypeClassId = copy.CommonTypeClassId;
            
            this.CommonTypeId = copy.CommonTypeId;
        }

        private string implementationTypeName;
        public string ImplementationTypeName
        {
            get
            {
                if (this.implementationType != null)
                {
                    return this.implementationType.FullName;
                }

                return this.implementationTypeName;
            }
            set
            {
                if (this.implementationType != null)
                {
                    return;
                }

                this.implementationTypeName = value;
            }
        }

        public double Ordinal { get; set; }

        public IEnumerable<ExportInformation> ServiceExports { get; set; }

        private Type implementationType;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public Type ImplementationType
        {
            get
            {
                if (this.implementationType == null
                    && this.implementationTypeName != null)
                {
                    this.implementationType = Type.GetType(this.implementationTypeName);
                }

                return this.implementationType;
            }
            set
            {
                this.implementationType = value;
            }
        }

        private object implementationInstance;
        public object ImplementationInstance
        {
            get
            {
                return this.implementationInstance;
            }
            set
            {
                this.implementationInstance = value;
                this.DisableLifeTimeManagement = this.implementationInstance != null;
            }
        }

        public SharedInstanceType SharedInstanceType
        {
            get;
            set;
        }

        public bool DisableLifeTimeManagement
        {
            get;
            set;
        }

        public Guid UserGroupId { get; set; }

        public bool InjectProperties { get; set; }

        public IEnumerable<PropertyValue> ConstructorParameterValues { get; set; }

        public IEnumerable<PropertyValue> PropertyValues { get; set; }

        public Guid CommonTypeId { get; set; }

        public Guid CommonTypeClassId { get; set; }
    }

    public class ComponentRegistrationInfoCollection
    {
        public ComponentRegistrationInfoCollection(UserGroupInformation userGroupInfo)
        {
            this.UserGroupInfo = userGroupInfo;
            this.Components = new List<ComponentRegistrationInfo>();
            //this.UnloadedComponents = new List<ComponentRegistrationInfo>();
        }

        public UserGroupInformation UserGroupInfo { get; protected set; }

        public List<ComponentRegistrationInfo> Components { get; private set; }

        //public List<ComponentRegistrationInfo> UnloadedComponents { get; private set; }
    }

    public class PropertyValue
    {
        public PropertyValue()
        {
        }

        public PropertyValue(PropertyValue copy)
        {
            this.Name = copy.Name;
            this.ResolutionKey = copy.ResolutionKey;
            this.ResolutionType = copy.ResolutionType;
            this.Value = copy.Value;
            this.ValueGetter = copy.ValueGetter;
            this.ValueType = copy.ValueType;
        }

        public Type ValueType { get; set; }

        public string Name { get; set; }

        public object Value { get; set; }

        public Func<object> ValueGetter { get; set; }

        public object ResolutionKey { get; set; }

        public Type ResolutionType { get; set; }
    }
}
