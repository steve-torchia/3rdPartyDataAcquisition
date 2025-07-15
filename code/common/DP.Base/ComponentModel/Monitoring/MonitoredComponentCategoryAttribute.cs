using System;
using System.Collections.Generic;

namespace DP.Base.Monitoring
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class MonitoredComponentCategoryAttribute : System.Attribute
    {
        public MonitoredComponentCategoryAttribute(string categoryNames)
        {
            this.CategoryNames = categoryNames.Split(new char[] { ' ', ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
        }

        public IEnumerable<string> CategoryNames { get; set; }
    }
}
