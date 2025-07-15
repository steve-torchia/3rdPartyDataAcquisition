using System;
using System.Collections.Generic;
using System.Threading;

namespace DP.Base.Collections
{
    public class ConcurrentWeakReferenceCollection<T> : ICollection<T>
        where T : class
    {
        private List<WeakReference<T>> innerList;
        private ReaderWriterLockSlim innerListRWL;

        public ConcurrentWeakReferenceCollection()
        {
            this.innerList = new List<WeakReference<T>>();
            this.innerListRWL = new ReaderWriterLockSlim();
        }

        public bool CleanDeadRefs()
        {
            this.innerListRWL.EnterWriteLock();
            try
            {
                return this.CleanDeadRefsUnsafe(null);
            }
            finally
            {
                this.innerListRWL.ExitWriteLock();
            }
        }

        private bool CleanDeadRefsUnsafe(T removeItem)
        {
            bool retVal = false;
            for (int i = this.innerList.Count - 1; i >= 0; i--)
            {
                var item = this.innerList[i];
                if (item == null ||
                    item == removeItem)
                {
                    if (removeItem != null && item == removeItem)
                    {
                        retVal = true;
                    }

                    this.innerList.RemoveAt(i);
                }
            }

            return retVal;
        }

        public List<T> GetLocalList()
        {
            List<T> localList = new List<T>();
            this.innerListRWL.EnterReadLock();
            try
            {
                for (int i = 0; i < this.innerList.Count; i++)
                {
                    T tmpItem = default(T);
                    if (this.innerList[i].TryGetTarget(out tmpItem) == false ||
                        tmpItem == null)
                    {
                        continue;
                    }

                    localList.Add(tmpItem);
                }
            }
            finally
            {
                this.innerListRWL.ExitReadLock();
            }

            return localList;
        }

        public T GetItem(Func<T, bool> findItem)
        {
            List<T> localList = new List<T>();
            this.innerListRWL.EnterReadLock();
            try
            {
                for (int i = 0; i < this.innerList.Count; i++)
                {
                    T tmpItem;
                    if (this.innerList[i].TryGetTarget(out tmpItem) == false ||
                        tmpItem == null)
                    {
                        continue;
                    }

                    if (findItem(tmpItem))
                    {
                        return tmpItem;
                    }
                }
            }
            finally
            {
                this.innerListRWL.ExitReadLock();
            }

            return default(T);
        }

        #region ICollection<T> Members

        public void Add(T item)
        {
            this.innerListRWL.EnterWriteLock();
            try
            {
                this.CleanDeadRefsUnsafe(null);
                this.innerList.Add(new WeakReference<T>(item));
            }
            finally
            {
                this.innerListRWL.ExitWriteLock();
            }
        }

        public void Clear()
        {
            this.innerListRWL.EnterWriteLock();
            try
            {
                this.innerList.Clear();
            }
            finally
            {
                this.innerListRWL.ExitWriteLock();
            }
        }

        public bool Contains(T item)
        {
            this.innerListRWL.EnterReadLock();
            try
            {
                return this.innerList.Find(a =>
                {
                    T tmpItem;
                    return a.TryGetTarget(out tmpItem) && tmpItem == item;
                }) != null;
            }
            finally
            {
                this.innerListRWL.ExitReadLock();
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            var list = this.GetLocalList();
            list.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return this.innerList.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(T item)
        {
            this.innerListRWL.EnterWriteLock();
            try
            {
                return this.CleanDeadRefsUnsafe(item);
            }
            finally
            {
                this.innerListRWL.ExitWriteLock();
            }
        }

        #endregion

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            return this.GetLocalList().GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion
    }
}