using System;
using System.Collections.Generic;
using DP.Base.Contracts;

namespace DP.Base.ComponentModel
{
    public class CategorizedComponentComparer : IComparer<ICategorizedComponent>
    {
        private static volatile CategorizedComponentComparer instance;
        public static CategorizedComponentComparer Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (typeof(CategorizedComponentComparer))
                    {
                        if (instance == null)
                        {
                            instance = new CategorizedComponentComparer();
                        }
                    }
                }

                return instance;
            }
        }

        #region IComparer<ICategorizedComponent> Members

        private StringComparer stringComparer = StringComparer.Create(System.Globalization.CultureInfo.CurrentCulture, false);
        public int Compare(ICategorizedComponent x, ICategorizedComponent y)
        {
            if (x == null)
            {
                if (y == null)
                {
                    return 0;
                }

                return 1;
            }

            if (y == null)
            {
                return -1;
            }

            int xCatCount = (x.ComponentCategories == null) ? 0 : x.ComponentCategories.Count;
            int yCatCount = (y.ComponentCategories == null) ? 0 : y.ComponentCategories.Count;
            int maxCount = System.Math.Max(xCatCount, yCatCount);

            int retVal = 0;
            for (int i = 0; i < maxCount; i++)
            {
                string xVal = (i < xCatCount) ? x.ComponentCategories[i] : null;
                string yVal = (i < yCatCount) ? y.ComponentCategories[i] : null;
                retVal = this.stringComparer.Compare(xVal, yVal);
                if (retVal != 0)
                {
                    return retVal;
                }
            }

            return this.stringComparer.Compare(x.Name, y.Name);
        }

        #endregion
    }
}
