using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using DP.Base.Contracts;
using DP.Base.Contracts.ComponentModel;
using DP.Base.Contracts.Logging;
using DP.Base.Contracts.Security;
using DP.Base.Contracts.ServiceLocator;

namespace DP.Base.Context
{
    public abstract partial class ContextBase : IContext
    {
        protected ContextBase()
        {
        }

        public virtual void Initialize(IContext parentContext, IContextContainer container, UserGroupInformation userGroupInfo)
        {
            this.Parent = parentContext;
            this.Container = container;

            if (userGroupInfo != null)
            {
                this.userGroupInfo = userGroupInfo;
            }
            else if (parentContext != null && parentContext.UserGroupInfo != null)
            {
                this.userGroupInfo = parentContext.UserGroupInfo;
            }

            container.Context = this;
            container.Disposing += this.Container_Disposing;
        }

        private bool isInitializeAfterCreate;
        public virtual void InitializeAfterCreate()
        {
            if (this.isInitializeAfterCreate == true)
            {
                return;
            }

            this.isInitializeAfterCreate = true;
        }

        protected UserGroupInformation userGroupInfo;
        public virtual UserGroupInformation UserGroupInfo
        {
            get
            {
                if (this.userGroupInfo != null)
                {
                    return this.userGroupInfo;
                }

                var parentOC = this.Parent as IObjectContext;

                if (parentOC != null)
                {
                    return parentOC.UserGroupInfo;
                }

                return null;
            }
        }

        private string definedName;
        public virtual string Name
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this.definedName) == false)
                {
                    return this.definedName;
                }

                if (this.Container is INamedComponent)
                {
                    return ((INamedComponent)this.Container).Name;
                }

                return this.Container.GetType().FullName;
            }
            set
            {
                this.definedName = value;
            }
        }

        private IDiagnostic diagnostic;
        public virtual IDiagnostic Diagnostic
        {
            get
            {
                if (this.diagnostic != null)
                {
                    return this.diagnostic;
                }

                var tmpDiag = this.CreateDiagnostic();
                this.diagnostic = tmpDiag;

                return this.diagnostic;
            }
            set
            {
                this.diagnostic = value;
            }
        }

        protected virtual IDiagnostic CreateDiagnostic()
        {
            var cur = ObjectContextActionScope.CurrentObjectContext;
            if (cur != null && cur.Diagnostic != null)
            {
                var tmpDiag = new DiagnosticWrapper();
                tmpDiag.Initialize(cur.Diagnostic, this);
                return tmpDiag;
            }

            return null;
        }

        public override string ToString()
        {
            return string.Format("ObjectContext: {0}", this.Name);
        }

        protected ILoggerFactory loggerFactory;

        public virtual ILoggerFactory LoggerFactory
        {
            get
            {
                if (this.loggerFactory == null && this.Parent != null)
                {
                    return this.Parent.LoggerFactory;
                }

                return this.loggerFactory;
            }
            set
            {
                this.loggerFactory = value;
            }
        }

        private ILogger log;
        public virtual ILogger Log
        {
            get
            {
                if (this.log == null)
                {
                    lock (this)
                    {
                        if (this.log == null)
                        {
                            this.log = this.LoggerFactory.GetLogger(this.Name);
                        }
                    }
                }

                return this.log;
            }
            set
            {
                this.log = value;
            }
        }

        ////// !RMM add a diagnostic.. collects diagnostice output text for object context, automaticaly logged as well
        ////// make it an ILogger so have all the same members.  Probably should put diags into the root diagnostic objectContext
        ////// defined by a diagnostic barrier, since most object context will have a path all the way to the global context

        public abstract IServiceLocatorProvider ServiceLocatorProvider { get; set; }

        public abstract IServiceLocator ServiceLocator { get; set; }

        public IContextContainer Container
        {
            get;
            protected set;
        }

        public IContext Parent { get; protected set; }

        protected DP.Base.Contracts.IDateTimeProvider dateTimeProvider;
        public virtual DP.Base.Contracts.IDateTimeProvider DateTimeProvider
        {
            get
            {
                if (this.dateTimeProvider != null)
                {
                    return this.dateTimeProvider;
                }

                if (this.Parent != null)
                {
                    return this.Parent.DateTimeProvider;
                }

                if (this.ServiceLocator.TryGetInstance<DP.Base.Contracts.IDateTimeProvider>(null, out this.dateTimeProvider) == false)
                {
                    this.dateTimeProvider = new DP.Base.Contracts.DateTimeProvider();
                }

                return this.dateTimeProvider;
            }
            set
            {
                this.dateTimeProvider = value;
            }
        }

        /// <summary>
        /// Implementation of Dispose according to .NET Framework Design Guidelines.
        /// </summary>
        /// <remarks>Do not make this method virtual.
        /// A derived class should not be able to override this method.
        /// </remarks>
        public void Dispose()
        {
            this.Dispose(true);

            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue 
            // and prevent finalization code for this object
            // from executing a second time.

            // Always use SuppressFinalize() in case a subclass
            // of this type implements a finalizer.
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool isDisposing)
        {
            // TODO If you need thread safety, use a lock around these 
            // operations, as well as in your methods that use the resource.
            try
            {
                if (!this.IsDisposed)
                {
                    if (isDisposing)
                    {
                        // TODO Release all managed resources here
                        this.DisposeCore(isDisposing);
                    }
                }
            }
            finally
            {
                // explicitly call the base class Dispose implementation
                this.IsDisposed = true;
            }
        }

        protected virtual void DisposeCore(bool isDisposing)
        {
            if (this.Disposing != null)
            {
                this.Disposing(this, EventArgs.Empty);
            }
        }

        public event EventHandler Disposing;

        public bool IsDisposed
        {
            get;
            protected set;
        }

        private ConcurrentDictionary<string, object> propertyBag;
        public ConcurrentDictionary<string, object> PropertyBag
        {
            get
            {
                if (this.propertyBag == null)
                {
                    lock (this)
                    {
                        if (this.propertyBag == null)
                        {
                            this.propertyBag = new ConcurrentDictionary<string, object>();
                        }
                    }
                }

                return this.propertyBag;
            }
        }

        protected virtual void Container_Disposing(object sender, EventArgs e)
        {
            var container = ((IContextContainer)sender);
            if (container != null && container.Context != null)
            {
                container.Context.Dispose();
            }
        }
    }
}
