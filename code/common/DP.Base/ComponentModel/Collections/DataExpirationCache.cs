using System;
using System.Collections.Generic;
using System.Threading;
using DP.Base.Contracts.Logging;
using DP.Base.Utilities;

namespace DP.Base.Collections
{
    public class DataExpirationCache<TObjectKey, TObjectValue, TUserData> : IDisposable
    {
        private volatile bool needsForExpired;
        private volatile bool checkingForExpired;
        private ulong lastCheckExpireTimeTicks;
        private Func<TObjectKey, TUserData, TObjectValue> findObjectFunc;
        private Func<List<ValueWrapper>> initializeFunc;
        private ReaderWriterLockSlim rwl;
        private Dictionary<TObjectKey, ValueWrapper> innerDictionary;
        private Dictionary<TObjectKey, object> findObjectSyncMap;
        private ILogger log;

        /// <summary>
        /// Gets or sets mSEC defines when the list should be checked for old items
        /// default check for expired every 10 minutes (600000).
        /// </summary>
        public ulong CheckExpireTimeTicks { get; set; }

        public bool ResetTimeoutOnAccess { get; set; }

        /// <summary>
        /// Gets or sets how long each object can be cached
        /// default each object lives for 10 minutes (600000).
        /// </summary>
        public ulong ValueExpireTimeout { get; set; }

        public DataExpirationCache(
            Func<TObjectKey, TUserData, TObjectValue> findObjectFunc,
            Func<List<ValueWrapper>> initializeFunc,
            ILogger log, 
            uint valueExpireTimeout = 600000)
        {
            //default check for expired every 10 minutes
            this.ValueExpireTimeout = this.CheckExpireTimeTicks = valueExpireTimeout;

            this.log = log;

            this.rwl = new ReaderWriterLockSlim();
            this.innerDictionary = new Dictionary<TObjectKey, ValueWrapper>();
            this.findObjectSyncMap = new Dictionary<TObjectKey, object>();
            this.findObjectFunc = findObjectFunc;
            this.initializeFunc = initializeFunc;

            this.Initialize();
            this.lastCheckExpireTimeTicks = EnvironmentEx.TickCount64;
        }

        public void MarkDirty(TObjectKey key)
        {
            this.rwl.EnterReadLock();
            try
            {
                ValueWrapper wrapper;
                if (this.innerDictionary.TryGetValue(key, out wrapper))
                {
                    wrapper.IsDirty = true;
                }
            }
            finally
            {
                this.rwl.ExitReadLock();
            }
        }

        public void ResetExpiration(TObjectKey key)
        {
            this.rwl.EnterReadLock();
            try
            {
                ValueWrapper wrapper;
                if (this.innerDictionary.TryGetValue(key, out wrapper))
                {
                    wrapper.AccessTick = EnvironmentEx.TickCount64;
                }
            }
            finally
            {
                this.rwl.ExitReadLock();
            }
        }

        public TObjectValue GetValue(TObjectKey key, TUserData userData)
        {
            this.rwl.EnterReadLock();
            try
            {
                TObjectValue retVal;
                if (this.LookupValue(key, out retVal))
                {
                    return retVal;
                }
            }
            finally
            {
                this.rwl.ExitReadLock();
            }

            TObjectValue newVal = default(TObjectValue);
            object findObjectFuncSync = null;
            lock (this.findObjectSyncMap)
            {
                if (this.findObjectSyncMap.TryGetValue(key, out findObjectFuncSync) == false)
                {
                    this.findObjectSyncMap[key] = findObjectFuncSync = new object();
                }
            }

            lock (findObjectFuncSync)
            {
                TObjectValue retVal;
                if (this.LookupValue(key, out retVal))
                {
                    return retVal;
                }

                newVal = this.findObjectFunc(key, userData);
                this.SetValue(key, newVal);
            }

            return newVal;
        }

        private bool LookupValue(TObjectKey key, out TObjectValue val)
        {
            val = default(TObjectValue);
            ValueWrapper wrapper;
            if (this.innerDictionary.TryGetValue(key, out wrapper))
            {
                if (wrapper.IsDirty == false)
                {
                    ulong nowTick = EnvironmentEx.TickCount64;
                    if (this.ValueExpireTimeout == 0 ||
                        EnvironmentEx.GetTickDelta(wrapper.AccessTick, nowTick) <= this.ValueExpireTimeout)
                    {
                        this.CheckExpired();
                        val = wrapper.Value;
                        if (this.ResetTimeoutOnAccess == true)
                        {
                            wrapper.ResetAccessTick();
                        }

                        return true;
                    }
                }
            }

            return false;
        }

        public void SetValue(TObjectKey key, TObjectValue value)
        {
            this.rwl.EnterWriteLock();
            try
            {
                ValueWrapper wrapper = new ValueWrapper(key, value);
                this.innerDictionary[key] = wrapper;
            }
            finally
            {
                this.rwl.ExitWriteLock();
            }

            this.CheckExpired();
        }

        public void RemoveValue(TObjectKey key)
        {
            this.rwl.EnterWriteLock();
            try
            {
                this.innerDictionary.Remove(key);
            }
            finally
            {
                this.rwl.ExitWriteLock();
            }

            lock (this.findObjectSyncMap)
            {
                this.findObjectSyncMap.Remove(key);
            }

            this.CheckExpired();
        }

        public void Clear()
        {
            this.rwl.EnterWriteLock();
            try
            {
                this.innerDictionary.Clear();
            }
            finally
            {
                this.rwl.ExitWriteLock();
            }

            lock (this.findObjectSyncMap)
            {
                this.findObjectSyncMap.Clear();
            }

            this.CheckExpired();
        }

        public virtual void Initialize()
        {
            if (this.initializeFunc == null)
            {
                return;
            }

            var wrapperList = this.initializeFunc();
            this.rwl.EnterWriteLock();
            try
            {
                this.innerDictionary.Clear();
                foreach (var wrapper in wrapperList)
                {
                    this.innerDictionary[wrapper.Key] = wrapper;
                }
            }
            finally
            {
                this.rwl.ExitWriteLock();
            }
        }

        private object checkingForExpiredSync = new object();
        private void CheckExpired()
        {
            if (this.ValueExpireTimeout == 0)
            {
                return;
            }

            if (this.needsForExpired == false &&
                this.checkingForExpired == false &&
                this.CheckExpireTimeTicks != 0 &&
                EnvironmentEx.GetTickDelta(this.lastCheckExpireTimeTicks, EnvironmentEx.TickCount64) > this.CheckExpireTimeTicks)
            {
                lock (this.checkingForExpiredSync)
                {
                    if (this.needsForExpired == false &&
                        this.checkingForExpired == false &&
                        this.CheckExpireTimeTicks != 0 &&
                        EnvironmentEx.GetTickDelta(this.lastCheckExpireTimeTicks, EnvironmentEx.TickCount64) > this.CheckExpireTimeTicks)
                    {
                        this.needsForExpired = true;
                        this.lastCheckExpireTimeTicks = EnvironmentEx.TickCount64;
                        ThreadPool.QueueUserWorkItem(new WaitCallback(this.CheckExpiredProc));
                    }
                }
            }
        }

        private object checkingForExpiredProcSync = new object();
        private void CheckExpiredProc(object state)
        {
            lock (this.checkingForExpiredProcSync)
            {
                if (this.needsForExpired == false || this.checkingForExpired == true)
                {
                    return;
                }

                this.checkingForExpired = true;
            }

            try
            {
                List<ValueWrapper> removeList = new List<ValueWrapper>();
                this.rwl.EnterReadLock();
                try
                {
                    ulong nowTick = EnvironmentEx.TickCount64;
                    foreach (var pair in this.innerDictionary)
                    {
                        if (pair.Value.IsDirty ||
                            (this.ValueExpireTimeout != 0 && EnvironmentEx.GetTickDelta(pair.Value.AccessTick, nowTick) > this.ValueExpireTimeout))
                        {
                            removeList.Add(pair.Value);
                        }
                    }
                }
                finally
                {
                    this.rwl.ExitReadLock();
                }

                if (removeList.Count == 0)
                {
                    return;
                }

                this.rwl.EnterWriteLock();
                try
                {
                    foreach (var wrapper in removeList)
                    {
                        this.innerDictionary.Remove(wrapper.Key);
                    }
                }
                finally
                {
                    this.rwl.ExitWriteLock();
                }
            }
            catch (Exception ex)
            {
                //no throw when in treadPool thread
                if (this.log != null && this.log.IsErrorEnabled)
                {
                    this.log.Error("CheckExpiredProc", ex);
                }
            }
            finally
            {
                this.needsForExpired = false;
                this.checkingForExpired = false;
            }
        }

        public class ValueWrapper
        {
            public ValueWrapper(TObjectKey key, TObjectValue value)
            {
                this.Key = key;
                this.Value = value;
                this.AccessTick = EnvironmentEx.TickCount64;
            }

            public TObjectKey Key { get; set; }
            public TObjectValue Value { get; set; }
            public bool IsDirty { get; set; }
            public ulong AccessTick { get; set; }

            public void ResetAccessTick()
            {
            }
        }

        protected bool isDisposed;
        #region IDisposable
        public void Dispose()
        {
            this.Dispose(true);

            // Use SupressFinalize in case a subclass of this type implements a finalizer.
            GC.SuppressFinalize(this);
        }

        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
        protected virtual void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            this.isDisposed = true;
        }

        // Use C# destructor syntax for finalization code.
        // This destructor will run only if the Dispose method
        // does not get called.
        // It gives your base class the opportunity to finalize.
        // Do not provide destructors in types derived from this class.
        ~DataExpirationCache()
        {
            this.Dispose(false);
        }
        #endregion
    }
}
