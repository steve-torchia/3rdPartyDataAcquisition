using System;

namespace DP.Base.ComponentModel
{
    [AttributeUsage(AttributeTargets.Method)]
    public class PropertyDescriptorMethodAttribute : System.Attribute
    {
        public PropertyDescriptorMethodAttribute()
        {
        }

        public string DefaultDisplayText
        {
            get;
            set;
        }
    }
}
