using System;
using System.Collections.Generic;
using System.ComponentModel;
using DP.Base.Contracts;

namespace DP.Base.ComponentModel
{
    public class ProxyPropertyDescriptor : PropertyDescriptor
    {
        private PropertyDescriptor innerPropertyDescriptor;

        public ProxyPropertyDescriptor(PropertyDescriptor innerPropertyDescriptor,
                        Func<object, object> getSetComponentConverter)
            : base(innerPropertyDescriptor.Name, GetAttributeAr(innerPropertyDescriptor))
        {
            this.GetSetComponentConverter = getSetComponentConverter;
            this.AutoCastSetToInnerType = true;
            this.innerPropertyDescriptor = innerPropertyDescriptor;
        }

        public bool AutoCastSetToInnerType { get; set; }
        private static Attribute[] GetAttributeAr(PropertyDescriptor pd)
        {
            if (pd.Attributes == null || pd.Attributes.Count == 0)
            {
                return null;
            }

            Attribute[] retAr = new Attribute[pd.Attributes.Count];
            pd.Attributes.CopyTo(retAr, 0);

            return retAr;
        }

        public Func<object, object> GetSetComponentConverter
        {
            get;
            set;
        }

        //TypeDescriptor.CreateProperty(type, tuple.Item2.Name, tuple.Item2.PropertyType)
        public override bool CanResetValue(object component)
        {
            if (this.GetSetComponentConverter != null)
            {
                component = this.GetSetComponentConverter(component);
            }

            return this.innerPropertyDescriptor.CanResetValue(component);
        }

        public override Type ComponentType
        {
            get { return this.innerPropertyDescriptor.ComponentType; }
        }

        public override object GetValue(object component)
        {
            if (this.GetSetComponentConverter != null)
            {
                component = this.GetSetComponentConverter(component);
            }

            return this.innerPropertyDescriptor.GetValue(component);
        }

        public override bool IsReadOnly
        {
            get { return this.innerPropertyDescriptor.IsReadOnly; }
        }

        public override Type PropertyType
        {
            get { return this.innerPropertyDescriptor.PropertyType; }
        }

        public override void ResetValue(object component)
        {
            if (this.GetSetComponentConverter != null)
            {
                component = this.GetSetComponentConverter(component);
            }

            this.innerPropertyDescriptor.ResetValue(component);
        }

        public override void SetValue(object component, object value)
        {
            if (this.GetSetComponentConverter != null)
            {
                component = this.GetSetComponentConverter(component);
            }

            if (this.AutoCastSetToInnerType == true)
            {
                value = Convert.ChangeType(value, this.PropertyType);
            }

            this.innerPropertyDescriptor.SetValue(component, value);
        }

        public override bool ShouldSerializeValue(object component)
        {
            if (this.GetSetComponentConverter != null)
            {
                component = this.GetSetComponentConverter(component);
            }

            return this.innerPropertyDescriptor.ShouldSerializeValue(component);
        }
    }

    public class CategorizedProxyPropertyDescriptor : ProxyPropertyDescriptor, ICategorizedComponent
    {
        public CategorizedProxyPropertyDescriptor(PropertyDescriptor innerPropertyDescriptor, Func<object, object> getSetComponentConverter)
            : base(innerPropertyDescriptor, getSetComponentConverter)
        {
            this.ComponentCategories = new List<string>();
        }

        public List<string> ComponentCategories { get; private set; }
    }
}
