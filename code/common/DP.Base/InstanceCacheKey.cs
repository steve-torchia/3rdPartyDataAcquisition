using System;

namespace DP.Base
{ 
    public class InstanceCacheKey
    {
        public InstanceCacheKey(Type loggerConcreteType, string name)
        {
            this.ConcreteType = loggerConcreteType;
            this.Name = name;
        }

        public override bool Equals(object o)
        {
            InstanceCacheKey objA = o as InstanceCacheKey;
            if (object.ReferenceEquals(objA, null))
            {
                return false;
            }

            return ((this.ConcreteType == objA.ConcreteType) && (objA.Name == this.Name));
        }

        public override int GetHashCode()
        {
            return (this.ConcreteType ?? typeof(InstanceCacheKey)).GetHashCode() ^ (this.Name ?? string.Empty).GetHashCode();
        }

        public Type ConcreteType { get; private set; }

        public string Name { get; private set; }
    }
}