using System;
using System.Globalization;
using System.Reflection;

namespace DP.Base.Reflection
{
    public static class ReflectionHelper
    {
        public static object MethodInvoke(MethodInfo methodInfo, object target, object[] parameters)
        {
            return MethodInvoke(methodInfo, target, BindingFlags.Default, null, parameters, null);
        }

        public static object MethodInvoke(MethodInfo methodInfo, object target, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
        {
            try
            {
                return methodInfo.Invoke(target, invokeAttr, binder, parameters, culture);
            }
            catch (TargetInvocationException tiex)
            {
                if (tiex.InnerException == null)
                {
                    throw;
                }
                else
                {
                    FieldInfo remoteStackTraceString = typeof(Exception).GetField("_remoteStackTraceString", BindingFlags.Instance | BindingFlags.NonPublic);

                    // Set the InnerException._remoteStackTraceString to the current InnerException.StackTrace
                    remoteStackTraceString.SetValue(tiex.InnerException, tiex.InnerException.StackTrace + Environment.NewLine);

                    // Throw the new exception
                    throw tiex.InnerException;
                }
            }
        }

        public static object ConstructorInvoke(ConstructorInfo constructorInfo, object[] parameters)
        {
            return ConstructorInvoke(constructorInfo, BindingFlags.Default, null, parameters, null);
        }

        public static object ConstructorInvoke(ConstructorInfo constructorInfo, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
        {
            try
            {
                return constructorInfo.Invoke(invokeAttr, binder, parameters, culture);
            }
            catch (TargetInvocationException tiex)
            {
                if (tiex.InnerException == null)
                {
                    throw;
                }

                FieldInfo remoteStackTraceString = typeof(Exception).GetField("_remoteStackTraceString", BindingFlags.Instance | BindingFlags.NonPublic);

                // Set the InnerException._remoteStackTraceString to the current InnerException.StackTrace
                remoteStackTraceString.SetValue(tiex.InnerException, tiex.InnerException.StackTrace + Environment.NewLine);

                // Throw the new exception
                throw tiex.InnerException;
            }
        }

        public static void SetPrivateFieldValue<T, TProp>(T obj, string name, TProp val)
        {
            var field = typeof(T).GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(obj, val);
        }

        public static void SetPrivatePropertyValue<T, TProp>(T obj, string name, TProp val)
        {
            var prop = typeof(T).GetProperty(name, BindingFlags.NonPublic | BindingFlags.Instance);
            prop.SetValue(obj, val, null);
        }

        public static object GetPrivatePropertyValue<T>(T obj, string name)
        {
            var prop = typeof(T).GetProperty(name, BindingFlags.NonPublic | BindingFlags.Instance);
            return prop.GetValue(obj);
        }
    }
}
