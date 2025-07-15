using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DP.Base.Math
{
    public class BestFit
    {
        public class XYPoint
        {
            public double X;
            public double Y;
        }

        public void GenerateLinearBestFit(IEnumerable<XYPoint> points, out double slope, out double yOrigin)
        {
            this.GenerateLinearBestFit<XYPoint>(points,
                                                p => p.X,
                                                p => p.Y,
                                                out slope,
                                                out yOrigin);
        }

        public void GenerateLinearBestFit<T>(IEnumerable<T> points, Func<T, double> getXFunc, Func<T, double> getYFunc, out double slope, out double yOrigin)
        {
            int numPoints = points.Count();
            double meanX = points.Average(point => getXFunc(point));
            double meanY = points.Average(point => getYFunc(point));

            double sumXSquared = 0.0;
            double sumXY = 0.0;

            foreach (var point in points)
            {
                var x = getXFunc(point);
                var y = getYFunc(point);

                sumXSquared += x * x;
                sumXY += x * y;
            }

            slope = (sumXY / numPoints - meanX * meanY) / (sumXSquared / numPoints - meanX * meanX);
            yOrigin = -(slope * meanX - meanY);
        }

        public List<XYPoint> GenerateLinearBestFitLine(IEnumerable<XYPoint> points, out double slope, out double yOrigin)
        {
            return this.GenerateLinearBestFitLine<XYPoint>(points,
                                                p => p.X,
                                                p => p.Y,
                                                out slope,
                                                out yOrigin);
        }

        public List<XYPoint> GenerateLinearBestFitLine<T>(IEnumerable<T> points, Func<T, double> getXFunc, Func<T, double> getYFunc, out double slope, out double yOrigin)
        {
            this.GenerateLinearBestFit<T>(points, getXFunc, getYFunc, out slope, out yOrigin);

            double a1 = slope;
            double b1 = -yOrigin;

            return points.Select(point =>
            {
                var x = getXFunc(point);
                return new XYPoint() { X = x, Y = a1 * x - b1 };
            }).ToList();
        }
    }
}
