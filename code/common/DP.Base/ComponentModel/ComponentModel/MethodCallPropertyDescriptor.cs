using System;
using System.ComponentModel;
using System.Reflection;

namespace DP.Base.ComponentModel
{
    public class MethodCallPropertyDescriptor<TInArgType> : PropertyDescriptor
    {
        private Func<TInArgType, ParameterInfo[], object[]> parameterConverter;
        private MethodInfo methodInfo;
        private Type targetType;
        private PropertyDescriptorMethodAttribute attribute;
        public MethodCallPropertyDescriptor(
            Type targetType, 
            MethodInfo methodInfo,
            PropertyDescriptorMethodAttribute attribute,
            Func<TInArgType, ParameterInfo[], object[]> parameterConverter)
            : base(methodInfo.Name, null)
        {
            this.targetType = targetType;
            this.methodInfo = methodInfo;
            this.attribute = attribute;
            this.parameterConverter = parameterConverter;
        }

        public override bool CanResetValue(object component)
        {
            return false;
        }

        public override Type ComponentType
        {
            get { return this.targetType; }
        }

        public override object GetValue(object component)
        {
            if (this.attribute != null)
            {
                return this.attribute.DefaultDisplayText;
            }

            return null;
        }

        public override bool IsReadOnly
        {
            get { return false; }
        }

        public override Type PropertyType
        {
            get { return typeof(TInArgType); }
        }

        public override void ResetValue(object component)
        {
        }

        public override void SetValue(object component, object value)
        {
            TInArgType inArg = (TInArgType)Convert.ChangeType(value, typeof(TInArgType));

            object[] parameters = null;
            if (this.parameterConverter != null)
            {
                parameters = this.parameterConverter(inArg, this.methodInfo.GetParameters());
            }
            else
            {
                parameters = new object[] { value };
            }

            this.methodInfo.Invoke(component, parameters);
        }

        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }
    }
}
