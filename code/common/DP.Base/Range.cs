using System;
using Newtonsoft.Json;

namespace DP.Base
{
    public struct Range<T> : IComparable<Range<T>>, IEquatable<Range<T>>
        where T : struct, IComparable<T>, IEquatable<T>
    {
        [JsonConstructor]
        public Range(T minimum, T maximum)
        {
            this.Minimum = minimum;
            this.Maximum = maximum;
        }

        public T Minimum { get; private set; }
        public T Maximum { get; private set; }

        public override string ToString() => $"Range({this.Minimum}, {this.Maximum})";
        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                hash = hash * 23 + this.Minimum.GetHashCode();
                hash = hash * 23 + this.Maximum.GetHashCode();
                return hash;
            }
        }

        public override bool Equals(object o)
        {
            if (o is Range<T>)
            {
                return this.Equals((Range<T>)o);
            }

            return false;
        }

        public bool Equals(Range<T> r) => this.Minimum.Equals(r.Minimum) && this.Maximum.Equals(r.Maximum);

        public static bool operator ==(Range<T> lhs, Range<T> rhs) => lhs.Equals(rhs);
        public static bool operator !=(Range<T> lhs, Range<T> rhs) => !lhs.Equals(rhs);

        public int CompareTo(Range<T> r)
        {
            var comparision = this.Minimum.CompareTo(r.Minimum);
            if (comparision != 0)
            {
                return comparision;
            }

            comparision = this.Maximum.CompareTo(r.Maximum);
            if (comparision != 0)
            {
                return comparision;
            }

            return 0;
        }

        public bool Contains(T value) => this.Minimum.CompareTo(value) <= 0 && this.Maximum.CompareTo(value) >= 0;
        public bool Contains(Range<T> r) => this.Minimum.CompareTo(r.Minimum) <= 0 && this.Maximum.CompareTo(r.Maximum) >= 0;
    }
}