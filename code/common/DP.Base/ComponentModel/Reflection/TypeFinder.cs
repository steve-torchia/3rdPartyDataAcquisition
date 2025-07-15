using System;
using System.Collections.Generic;

namespace DP.Base.Reflection
{
    public class TypeFinder : IList<Type>
    {
        private List<Type> types;

        public TypeFinder(string types)
            : this(types, true)
        {
        }

        public TypeFinder(string types, bool throwOnMissingType)
        {
            this.types = new List<Type>(GetTypeListFromString(types, throwOnMissingType));
        }

        #region IList<Type> Members

        public int IndexOf(Type item)
        {
            return this.types.IndexOf(item);
        }

        public void Insert(int index, Type item)
        {
            this.types.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            this.types.RemoveAt(index);
        }

        public Type this[int index]
        {
            get
            {
                return this.types[index];
            }
            set
            {
                this.types[index] = value;
            }
        }

        #endregion

        #region ICollection<Type> Members

        public void Add(Type item)
        {
            this.types.Add(item);
        }

        public void Clear()
        {
            this.types.Clear();
        }

        public bool Contains(Type item)
        {
            return this.types.Contains(item);
        }

        public void CopyTo(Type[] array, int arrayIndex)
        {
            this.types.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return this.types.Count; }
        }

        public bool IsReadOnly
        {
            get { return ((ICollection<Type>)this.types).IsReadOnly; }
        }

        public bool Remove(Type item)
        {
            return this.types.Remove(item);
        }

        #endregion

        #region IEnumerable<Type> Members

        public IEnumerator<Type> GetEnumerator()
        {
            return this.types.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.types.GetEnumerator();
        }

        #endregion

        private static Type[] GetTypeListFromString(string types, bool throwOnMissingType)
        {
            string[] typeAr = types.Split(';');
            Type[] retVal = new Type[typeAr.Length];
            for (int i = 0; i < retVal.Length; i++)
            {
                string type = typeAr[i];
                retVal[i] = Type.GetType(type);
                if (throwOnMissingType && retVal[i] == null)
                {
                    throw new TypeLoadException(string.Format("could not load:{0}", type));
                }
            }

            return retVal;
        }
    }
}