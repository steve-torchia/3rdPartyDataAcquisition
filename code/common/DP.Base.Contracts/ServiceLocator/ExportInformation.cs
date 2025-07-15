using System;
using System.Diagnostics;

namespace DP.Base.Contracts.ServiceLocator
{
    public class ExportInformation
    {
        public ExportInformation()
        {
        }

        public ExportInformation(ExportInformation copy)
        {
            this.serviceAssemblyQualifiedType = copy.serviceAssemblyQualifiedType;
            this.ServiceKey = copy.serviceType;
            this.serviceType = copy.serviceType;
            this.ExportRegistrationType = copy.ExportRegistrationType;
            this.Ordinal = copy.Ordinal;
        }

        private Type serviceType;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public Type ServiceType
        {
            get
            {
                if (this.serviceType == null
                    && this.serviceAssemblyQualifiedType != null)
                {
                    this.serviceType = Type.GetType(this.serviceAssemblyQualifiedType);
                    if (this.serviceType == null)
                    {
                        throw new TypeLoadException("Could not load type:" + this.serviceAssemblyQualifiedType);
                    }
                }

                return this.serviceType;
            }
            set
            {
                this.serviceType = value;
            }
        }

        private string serviceAssemblyQualifiedType;
        public string ServiceAssemblyQualifiedType
        {
            get
            {
                if (this.serviceType != null)
                {
                    return this.serviceType.FullName;
                }

                return this.serviceAssemblyQualifiedType;
            }
            set
            {
                if (this.serviceType != null)
                {
                    return;
                }

                this.serviceAssemblyQualifiedType = value;
            }
        }

        public object ServiceKey
        {
            get;
            set;
        }

        public ExportRegistrationType ExportRegistrationType { get; set; }

        public double Ordinal { get; set; }
    }
}
