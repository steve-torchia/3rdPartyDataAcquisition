using System;
using System.ComponentModel;

namespace DP.Base.ComponentModel
{
    public class ReadOnlyPropertyDescriptor : PropertyDescriptor
    {
        private object value;
        public ReadOnlyPropertyDescriptor(string name, object value)
            : base(name, null)
        {
            this.value = value;
        }

        public override bool CanResetValue(object component)
        {
            return false;
        }

        public override Type ComponentType
        {
            get
            {
                return (this.value != null) ? this.value.GetType() : typeof(object);
            }
        }

        public override object GetValue(object component)
        {
            return this.value;
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
