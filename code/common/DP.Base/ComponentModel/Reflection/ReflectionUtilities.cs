using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace DP.Base.Reflection
{
    public static class ReflectionUtilities
    {
        public static string GetTypeName(Type type)
        {
            if (type.IsGenericType == false)
            {
                return type.Name;
            }

            StringBuilder sb = new StringBuilder();
            GetTypeName(type, sb);
            return sb.ToString();
        }

        /// <summary>
        /// Very rough way to get the name of a property without initializing the containing type.
        /// Usage: GetPropertyname((Type x) => x.Property)
        /// </summary>
        public static string GetPropertyName<T, TResult>(Expression<Func<T, TResult>> propertyAccessLambda)
        {
            var expressionBody = propertyAccessLambda.Body;

            if (!expressionBody.NodeType.Equals(ExpressionType.MemberAccess))
            {
                throw new Exception($"The lambda {expressionBody.ToString()} is not a Member Access expression. A member access expression has the form \"SOMETYPE.MEMBER\"");
            }

            var memberExpression = (MemberExpression)expressionBody;

            return memberExpression.Member.Name;
        }

        private static void GetTypeName(Type type, StringBuilder sb)
        {
            sb.Append(type.Name);
            if (type.IsGenericType == false)
            {
                return;
            }

            foreach (var arg in type.GenericTypeArguments)
            {
                sb.Append('[');
                GetTypeName(arg, sb);
                sb.Append(']');
            }
        }

        public static List<Type> FindAllDerivedTypes<T>(Assembly assembly)
        {
            try
            {
                var derivedType = typeof(T);
                return assembly
                    .GetTypes()
                    .Where(t => t != derivedType && derivedType.IsAssignableFrom(t))
                    .ToList();
            }
            catch (Exception /*ex*/)
            {
                return new List<Type>();
            }
        }
    }
}
