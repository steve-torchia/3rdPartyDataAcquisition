using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DP.Base.Collections
{
    public class LockableList<T> : ILockableList<T>, IList<T>, IList
    {
        private IList<T> innerList;
        private bool isReadOnly;

        public LockableList(IList<T> innerList, bool locked = false)
        {
            this.innerList = innerList;
            this.isReadOnly = locked;
        }

        public void Lock()
        {
            this.isReadOnly = true;
        }

        public T this[int index]
        {
            get
            {
                return this.innerList[index];
            }
            set
            {
                if (this.isReadOnly == true)
                {
                    throw new InvalidOperationException("cannot changed locked list");
                }

                this.innerList[index] = value;
            }
        }

        public int Count => this.innerList.Count;

        public bool IsReadOnly
        {
            get
            {
                return this.isReadOnly;
            }
        }

        public bool IsFixedSize
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public object SyncRoot
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsSynchronized
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        object IList.this[int index]
        {
            get
            {
                return this[index];
            }

            set
            {
                this[index] = (T)value;
            }
        }

        public void Add(T item)
        {
            if (this.isReadOnly == true)
            {
                throw new InvalidOperationException("cannot changed locked list");
            }

            this.innerList.Add(item);
        }

        public void Clear()
        {
            if (this.isReadOnly == true)
            {
                throw new InvalidOperationException("cannot changed locked list");
            }

            this.Clear();
        }

        public bool Contains(T item)
        {
            return this.innerList.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            this.innerList.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this.innerList.GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return this.innerList.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            if (this.isReadOnly == true)
            {
                throw new InvalidOperationException("cannot changed locked list");
            }

            this.innerList.Insert(index, item);
        }

        public bool Remove(T item)
        {
            if (this.isReadOnly == true)
            {
                throw new InvalidOperationException("cannot changed locked list");
            }

            return this.innerList.Remove(item);
        }

        public void RemoveAt(int index)
        {
            if (this.isReadOnly == true)
            {
                throw new InvalidOperationException("cannot changed locked list");
            }

            this.innerList.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.innerList.GetEnumerator();
        }

        public int Add(object value)
        {
            int count = this.Count;
            this.Add((T)value);
            return count;
        }

        public bool Contains(object value)
        {
            return this.Contains((T)value);
        }

        public int IndexOf(object value)
        {
            return this.IndexOf((T)value);
        }

        public void Insert(int index, object value)
        {
            this.Insert(index, (T)value);
        }

        public void Remove(object value)
        {
            this.Remove((T)value);
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }
    }

    public interface ILockableList<T> : IList<T>
    {
        void Lock();
    }
}
