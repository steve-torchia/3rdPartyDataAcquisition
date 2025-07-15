using System;
using System.Collections.Generic;
using System.Threading;

namespace DP.Base.Contracts.DataContractUtilities
{
    /// <summary>
    /// probably should be a singleton.. but for now just use a static class
    /// </summary>
    public static partial class KnownCommonTypes
    {
        private static object commonTypeTypesListSync = new object();
        private static List<Type> commonTypeTypesList = new List<Type>();
        private static ReaderWriterLockSlim commonTypeTypesListRWL = new ReaderWriterLockSlim();
        public static int CommonTypeTypesListVersion { get; private set; }
        public static void AddType(Type type)
        {
            commonTypeTypesListRWL.EnterReadLock();
            try
            {
                if (commonTypeTypesList.Contains(type))
                {
                    return;
                }
            }
            finally
            {
                commonTypeTypesListRWL.ExitReadLock();
            }

            commonTypeTypesListRWL.EnterWriteLock();
            try
            {
                if (commonTypeTypesList.Contains(type))
                {
                    return;
                }

                commonTypeTypesList.Add(type);
            }
            finally
            {
                commonTypeTypesListRWL.ExitWriteLock();
            }

            if (commonTypeTypesList.Contains(type))
            {
                return;
            }

            CommonTypeTypesListVersion++;
        }

        public static List<Type> GetCommonTypeTypesList()
        {
            commonTypeTypesListRWL.EnterReadLock();
            try
            {
                return new List<Type>(commonTypeTypesList);
            }
            finally
            {
                commonTypeTypesListRWL.ExitReadLock();
            }
        }
    }
}