using System.Collections.Generic;
using DP.Base.ComponentModel;
using DP.Base.Contracts;
using DP.Base.Contracts.ComponentModel;

namespace DP.Base.Monitoring
{
    public class ReflectiveMonitor : IReflectiveMonitor
    {
        public ReflectiveMonitor()
        {
            this.MonitorId = MonitoredComponentManager.Instance.GetNextId();
        }

        public bool? HasTargetBeenDisposed
        {
            get
            {
                var hasBeenDisposedObj = this.Target as IDisposableEx;

                if (hasBeenDisposedObj != null)
                {
                    return hasBeenDisposedObj.IsDisposed;
                }

                return null;
            }
        }

        public virtual object Target
        {
            get;
            set;
        }

        public long MonitorId
        {
            get;
            private set;
        }

        public virtual List<string> ComponentCategories
        {
            get
            {
                if (this.Target == null)
                {
                    return null;
                }

                return GetComponentCategories(this.Target);
            }
        }

        public static List<string> GetComponentCategories(object target)
        {
            List<string> retList = new List<string>();
            var curType = target.GetType();
            bool cont = true;

            while (curType != null && cont)
            {
                if (retList.Count == 0 &&
                    (curType.BaseType == null ||
                    (curType.BaseType.FullName.ToUpperInvariant().Contains(target.GetType().FullName.Split('.')[0].ToUpperInvariant()) == false)))
                {
                    retList.Add(curType.Name);
                    cont = false;
                }

                var attribAr = curType.GetCustomAttributes(typeof(MonitoredComponentCategoryAttribute), false);
                if (attribAr != null && attribAr.Length > 0)
                {
                    var names = ((MonitoredComponentCategoryAttribute)attribAr[0]).CategoryNames;

                    if (retList.Count == 0)
                    {
                        retList.AddRange(names);
                    }
                    else
                    {
                        retList.InsertRange(0, names);
                    }
                }

                curType = curType.BaseType;
            }

            return retList;
        }

        public string Name
        {
            get
            {
                return this.BuildName();
            }
        }

        private string BuildName()
        {
            string name;
            int hashCode;

            name = null;
            hashCode = 0;

            if (this.Target != null)
            {
                hashCode = this.Target.GetHashCode();
                if (this.Target is INamedComponent)
                {
                    name = ((INamedComponent)this.Target).Name;
                }

                if (string.IsNullOrWhiteSpace(name))
                {
                    name = this.Target.GetType().FullName;
                }
            }
            else
            {
                name = this.GetType().FullName;
                hashCode = this.GetHashCode();
            }

            return string.Concat(name, "_", hashCode);
        }

        private System.ComponentModel.PropertyDescriptor[] properties;

        public virtual System.ComponentModel.PropertyDescriptor[] GetProperties()
        {
            if (this.properties == null && this.Target != null)
            {
                System.ComponentModel.PropertyDescriptor[] tmpPropAr = null;
                if (this.Target != null)
                {
                    List<System.ComponentModel.PropertyDescriptor> pdList = new List<System.ComponentModel.PropertyDescriptor>();
                    System.ComponentModel.PropertyDescriptor[] propAr = null;

                    propAr = PropertyDescriptorArrayPool.Instance.GetPropertyDescriptorArForType(this.Target);
                    for (int i = 0; i < propAr.Length; i++)
                    {
                        pdList.Add(propAr[i]);
                    }

                    if ((this.Target is IContextContainer) &&
                        ((IContextContainer)this.Target).Context != null &&
                        ((IContextContainer)this.Target).Context.Log != null)
                    {
                        propAr = PropertyDescriptorArrayPool.Instance.GetPropertyDescriptorArForType(((IContextContainer)this.Target).Context.Log);
                        for (int i = 0; i < propAr.Length; i++)
                        {
                            pdList.Add(new ProxyPropertyDescriptor(propAr[i], ConvertMonitoredToLog));
                        }
                    }

                    tmpPropAr = pdList.ToArray();
                }

                this.properties = tmpPropAr;
            }

            return this.properties;
        }

        private static object ConvertMonitoredToLog(object value)
        {
            return ((IContextContainer)value).Context.Log;
        }

        private bool addedToMonitoredComponentManager;
        public void InitializeAfterCreate()
        {
            lock (this)
            {
                if (this.addedToMonitoredComponentManager == true)
                {
                    return;
                }

                this.addedToMonitoredComponentManager = true;
                MonitoredComponentManager.Instance.AddComponent(this);
            }
        }
    }
}
