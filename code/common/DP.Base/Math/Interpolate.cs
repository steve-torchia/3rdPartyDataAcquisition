using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DP.Base.Math
{
    public class Interpolate
    {
        private double focusXVal;
        private double x0;
        private double x1;
        private double ratio;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x">x axis value solving for</param>
        /// <param name="x0">point 0 x value</param>
        /// <param name="x1">point 1 x value</param>
        public Interpolate(double x, double x0, double x1)
        {
            this.focusXVal = x;
            this.x0 = x0;
            this.x1 = x1;

            this.ratio = (this.focusXVal - this.x0) / (this.x1 - this.x0);            
        }

        public double Calculate(double y0, double y1)
        {
            var delta = (y1 - y0);
            return y0 + delta * this.ratio;
        }
    }
}
