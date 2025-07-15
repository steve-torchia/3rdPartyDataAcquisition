using System;
using System.ComponentModel;

namespace DP.Base.ComponentModel
{
    public class NoGetOrSetPropertyDescriptor : PropertyDescriptor
    {
        public NoGetOrSetPropertyDescriptor(string name)
            : base(name, null)
        {
        }

        public override bool CanResetValue(object component)
        {
            return false;
        }

        public override Type ComponentType
        {
            get { return typeof(object); }
        }

        public override object GetValue(object component)
        {
            return null;
        }

        public override bool IsReadOnly
        {
            get { return true; }
        }

        public override Type PropertyType
        {
            get { return typeof(object); }
        }

        public override void ResetValue(object component)
        {
        }

        public override void SetValue(object component, object value)
        {
        }

        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }
    }
}
