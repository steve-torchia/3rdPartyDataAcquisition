using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace DP.Base.Reflection
{
    public static class TypeDescriptorHelper
    {
#pragma warning disable SA1309 // Field names must not begin with underscore
#pragma warning disable SA1300 // Element must begin with upper-case letter
        private static ConstructorInfo _reflectPropertyDescriptorCI;
        private static ConstructorInfo reflectPropertyDescriptorCI
        {
            get
            {
                if (_reflectPropertyDescriptorCI == null)
                {
                    Assembly assembly = Assembly.GetAssembly(typeof(PropertyDescriptor));
                    Type reflectPdType = assembly.GetType("System.ComponentModel.ReflectPropertyDescriptor");

                    _reflectPropertyDescriptorCI = reflectPdType.GetConstructor(new Type[]
                                                                    {
                                                                        typeof(Type),
                                                                        typeof(string),
                                                                        typeof(Type),
                                                                        typeof(PropertyInfo),
                                                                        typeof(MethodInfo),
                                                                        typeof(MethodInfo),
                                                                        typeof(Attribute[]),
                                                                    });
                }

                return _reflectPropertyDescriptorCI;
            }
        }
#pragma warning restore SA1300 // Element must begin with upper-case letter
#pragma warning restore SA1309 // Field names must not begin with underscore

        public static Dictionary<string, PropertyDescriptor> GetPropertyDescriptors(Type type,
                                                                                        List<string> propertyNames,
                                                                                        bool nonPublicSetter)
        {
            Dictionary<string, PropertyDescriptor> retList = new Dictionary<string, PropertyDescriptor>();
            List<Tuple<string, PropertyInfo>> requestedProperties = new List<Tuple<string, PropertyInfo>>();

            foreach (var prop in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (propertyNames == null ||
                    propertyNames.Contains(prop.Name))
                {
                    requestedProperties.Add(new Tuple<string, PropertyInfo>(prop.Name, prop));
                }
            }

            if (nonPublicSetter == false)
            {
                foreach (var tuple in requestedProperties)
                {
                    retList.Add(tuple.Item1, TypeDescriptor.CreateProperty(type, tuple.Item2.Name, tuple.Item2.PropertyType));
                }
            }
            else
            {
                foreach (var tuple in requestedProperties)
                {
                    PropertyInfo propInfo = tuple.Item2;
                    MethodInfo setterMethod = propInfo.GetSetMethod(true);
                    if (setterMethod == null)
                    {
                        propInfo = propInfo.DeclaringType.GetProperty(tuple.Item2.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.SetProperty);
                    }

                    retList.Add(
                        tuple.Item1,
                        (PropertyDescriptor)reflectPropertyDescriptorCI.Invoke(new object[]
                        {
                            type,
                            tuple.Item2.Name,
                            tuple.Item2.PropertyType,
                            propInfo,
                            propInfo.GetGetMethod(true),
                            propInfo.GetSetMethod(true),
                            null,
                        }));
                }
            }

            //maybe properties are methods
            if (propertyNames != null && propertyNames.Count > retList.Count)
            {
                foreach (var methodInfo in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if (propertyNames.Contains(methodInfo.Name))
                    {
                        retList.Add(methodInfo.Name,
                            new Base.ComponentModel.MethodCallPropertyDescriptor<string>(type, methodInfo, null, null));
                    }
                }
            }

            return retList;
        }
    }
}
