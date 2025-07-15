//using System;
//using DP.Base.Contracts;
//using DP.Base.Contracts.Logging;
//using DP.Base.Utilities;

//namespace DP.Base.Collections
//{
//    public class DataVersionLinkedCache<TCacheType> : IDisposable
//        where TCacheType : class
//    {
//        private IDataHashProvider dataHashProvider;

//        private ulong dataVersion = 0;
//        private int lastForceEnsureCache;
//        private uint maxRefreshFrequency;
//        private const uint DefaultMaxRefreshFrequency = 5000; //5 sec

//        private volatile TCacheType cache;
//        private object cacheSync = new object();

//        private ILogger log;

//        public DataVersionLinkedCache(IDataHashProvider dataHashProvider,
//                                        Func<TCacheType> buildNewCachedObject)
//            : this(dataHashProvider, null, DefaultMaxRefreshFrequency, buildNewCachedObject)
//        {
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="dataHashProvider">dataHashProvider.</param>
//        /// <param name="log">can be null, will use global loggerFactory to create.</param>
//        /// <param name="buildNewCachedObject">buildNewCachedObject.</param>
//        public DataVersionLinkedCache(IDataHashProvider dataHashProvider,
//                                        ILogger log,
//                                        uint maxRefreshFrequency,
//                                        Func<TCacheType> buildNewCachedObject)
//        {
//            this.maxRefreshFrequency = maxRefreshFrequency;
//            this.dataHashProvider = dataHashProvider;
//            if (log == null)
//            {
//                this.log = ApplicationEx.Instance.GlobalContext.LoggerFactory.GetLogger(this.GetType());
//            }
//            else
//            {
//                this.log = log;
//            }

//            this.BuildNewCachedObject = buildNewCachedObject;
//        }

//        public ulong DataVersion
//        {
//            get
//            {
//                return this.dataVersion;
//            }
//        }

//        public bool HasCache
//        {
//            get { return this.cache != null; }
//        }

//        public void ClearCache()
//        {
//            IDisposable oldCache;
//            lock (this.cacheSync)
//            {
//                oldCache = this.cache as IDisposable;
//                this.cache = null;
//            }

//            try
//            {
//                if (oldCache != null)
//                {
//                    oldCache.Dispose();
//                }
//            }
//            catch (Exception ex)
//            {
//                if (this.log.IsWarnEnabled)
//                {
//                    this.log.Warn("error disposing cache", ex);
//                }
//            }
//        }

//        public TCacheType Cache
//        {
//            get
//            {
//                this.EnsureCache(false);
//                return this.cache;
//            }
//        }

//        private Func<TCacheType> BuildNewCachedObject { get; set; }

//        public void EnsureCache(bool force)
//        {
//            if (this.isDisposed ||
//                (force == false &&
//                this.cache != null &&
//                this.dataVersion == this.dataHashProvider.DataVersion))
//            {
//                return;
//            }

//            lock (this.cacheSync)
//            {
//                if ((this.isDisposed ||
//                    (force == false &&
//                    this.cache != null &&
//                    this.dataVersion == this.dataHashProvider.DataVersion)))
//                {
//                    return;
//                }

//                try
//                {
//                    var tmpDataVersion = this.dataHashProvider.DataVersion;

//                    if (force == false)
//                    {
//                        int tick = Environment.TickCount;
//                        if (EnvironmentEx.GetTickDelta(this.lastForceEnsureCache, tick) < this.maxRefreshFrequency)
//                        {
//                            return;
//                        }

//                        this.lastForceEnsureCache = tick;
//                    }

//                    var oldCache = this.cache;
//                    //set is atomic
//                    this.cache = this.BuildNewCachedObject();
//                    if (oldCache is IDisposable)
//                    {
//                        try
//                        {
//                            ((IDisposable)oldCache).Dispose();
//                        }
//                        catch (Exception ex)
//                        {
//                            if (this.log.IsDebugEnabled)
//                            {
//                                this.log.Debug("Error disposing old cache", ex);
//                            }
//                        }
//                    }

//                    this.dataVersion = tmpDataVersion;

//                    if (this.log.IsDebugEnabled)
//                    {
//                        this.log.Debug("Refreshed Cache: " + typeof(TCacheType).FullName);
//                    }
//                }
//                catch (Exception outerEx)
//                {
//                    try
//                    {
//                        this.log.Error("Error refreshing cache", outerEx);
//                    }
//                    catch
//                    {
//                    }
//                }
//            }
//        }

//        #region IDisposable Members

//        protected bool isDisposed;
//        #region IDisposable
//        public void Dispose()
//        {
//            this.Dispose(true);

//            // Use SupressFinalize in case a subclass of this type implements a finalizer.
//            GC.SuppressFinalize(this);
//        }

//        // If disposing equals false, the method has been called by the
//        // runtime from inside the finalizer and you should not reference
//        // other objects. Only unmanaged resources can be disposed.
//        protected virtual void Dispose(bool disposing)
//        {
//            if (this.isDisposed)
//            {
//                return;
//            }

//            this.isDisposed = true;
//            lock (this.cacheSync)
//            {
//                if (disposing)
//                {
//                    // dispose managed resources

//                    if (this.cache != null && (this.cache is IDisposable))
//                    {
//                        ((IDisposable)this.cache).Dispose();
//                    }
//                }

//                // dispose unmanaged resources
//            }
//        }

//        // Use C# destructor syntax for finalization code.
//        // This destructor will run only if the Dispose method
//        // does not get called.
//        // It gives your base class the opportunity to finalize.
//        // Do not provide destructors in types derived from this class.
//        ~DataVersionLinkedCache()
//        {
//            this.Dispose(false);
//        }
//        #endregion

//        #endregion
//    }
//}
