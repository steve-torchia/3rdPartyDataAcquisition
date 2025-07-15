using System;
using System.Linq;
using DP.Base.Extensions;

namespace DP.Base
{
    public class Coordinate
    {
        public float Latitude { get; set; }
        public float Longitude { get; set; }

        public Coordinate()
        {
        }

        public Coordinate(string latLongStr)
        {
            if (latLongStr.IsNullOrEmpty())
            {
                throw new Exception($"Incorrect Lat, Long string (null or empty)");
            }

            var tmp = latLongStr.Split(',');

            if (tmp.Count() != 2)
            {
                throw new Exception($"Incorrect Lat, Long string format: {latLongStr}");
            }

            this.Latitude = float.Parse(tmp[0]);
            this.Longitude = float.Parse(tmp[1]);
        }

        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(this, obj))
            {
                return true;
            }

            var other = obj as Coordinate;
            if (other == null)
            {
                return false;
            }

            return this.Latitude == other.Latitude && this.Longitude == other.Longitude;
        }

        public override int GetHashCode()
        {
            return this.Latitude.GetHashCode() ^ this.Longitude.GetHashCode();
        }

        public override string ToString()
        {
            //39.325248,-76.6615771
            return $"{this.Latitude},{this.Longitude}";
        }
    }
}