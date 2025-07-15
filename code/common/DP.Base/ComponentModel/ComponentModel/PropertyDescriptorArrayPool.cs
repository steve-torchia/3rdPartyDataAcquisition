using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DP.Base.Contracts.ComponentModel;

namespace DP.Base.ComponentModel
{
    public class PropertyDescriptorArrayPool
    {
        private PropertyDescriptorArrayPool()
        {
            this.innerMap = new System.Collections.Concurrent.ConcurrentDictionary<Type, System.ComponentModel.PropertyDescriptor[]>();
        }

        private static volatile PropertyDescriptorArrayPool instance;
        public static PropertyDescriptorArrayPool Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (typeof(PropertyDescriptorArrayPool))
                    {
                        if (instance == null)
                        {
                            instance = new PropertyDescriptorArrayPool();
                        }
                    }
                }

                return instance;
            }
        }

        private System.Collections.Concurrent.ConcurrentDictionary<Type, System.ComponentModel.PropertyDescriptor[]> innerMap;

        public System.ComponentModel.PropertyDescriptor[] GetPropertyDescriptorArForType(Type type)
        {
            System.ComponentModel.PropertyDescriptor[] retVal = null;
            if (this.innerMap.TryGetValue(type, out retVal))
            {
                return retVal;
            }

            var pdCollection = TypeDescriptor.GetProperties(type);
            List<PropertyDescriptor> pdList = new List<PropertyDescriptor>(pdCollection.Cast<PropertyDescriptor>());

            var methods = type.GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.FlattenHierarchy);
            foreach (var method in methods)
            {
                var attrib = method.GetCustomAttributes(typeof(PropertyDescriptorMethodAttribute), true);

                if (attrib.Length > 0)
                {
                    pdList.Add(new MethodCallPropertyDescriptor<string>(type, method, attrib[0] as PropertyDescriptorMethodAttribute, null));
                }
            }

            retVal = pdList.ToArray();
            this.innerMap.TryAdd(type, retVal);

            return retVal;
        }

        //
        public PropertyDescriptor[] GetPropertyDescriptorArForType(object item)
        {
            if (item is IPropertyDescriptorProvider)
            {
                return ((IPropertyDescriptorProvider)item).GetProperties();
            }

            return this.GetPropertyDescriptorArForType(item.GetType());
        }
    }
}
