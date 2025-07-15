using System;
using System.Collections.Generic;

namespace DP.Base.Extensions
{
    public static class ExceptionExtensions
    {
        public static Exception GetInnerMostException(this Exception ex)
        {
            Exception currentEx = ex;
            while (currentEx.InnerException != null)
            {
                currentEx = currentEx.InnerException;
            }

            return currentEx;
        }

        public static IEnumerable<string> GetAllChildExceptionMessages(this Exception ex)
        {
            var retVal = new List<string>();

            Exception currentEx = ex;

            if (currentEx == null)
            {
                return retVal;
            }

            if (!string.IsNullOrEmpty(currentEx.Message))
            {
                retVal.Add(currentEx.Message);
            }

            while (currentEx.InnerException != null)
            {
                currentEx = currentEx.InnerException;
                
                if (!string.IsNullOrEmpty(currentEx.Message))
                {
                    retVal.Add(currentEx.Message);
                }
            }

            return retVal;
        }
    }
}