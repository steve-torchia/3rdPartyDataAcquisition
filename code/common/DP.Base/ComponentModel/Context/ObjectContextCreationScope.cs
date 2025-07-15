using System;
using System.Collections.Generic;
using DP.Base.Contracts;

namespace DP.Base.Context
{
    public class ObjectContextCreationScope : IObjectContextScope
    {
#pragma warning disable SA1309 // Field names must not begin with underscore
#pragma warning disable SA1300 // Element must begin with upper-case letter
        [ThreadStatic]
        private static LinkedList<IObjectContext> _innerList;
        private static LinkedList<IObjectContext> innerList
        {
            get
            {
                if (_innerList == null)
                {
                    _innerList = new LinkedList<IObjectContext>();
                }

                return _innerList;
            }
        }
#pragma warning restore SA1300 // Element must begin with upper-case letter
#pragma warning restore SA1309 // Field names must not begin with underscore

        private IObjectContext objectContext;
        public ObjectContextCreationScope()
        {
        }

        private bool pushedContext;
        public IObjectContext PushContext(IObjectContext objectContext)
        {
            IObjectContext retVal = null;
            if (innerList.Count > 0)
            {
                retVal = innerList.Last.Value;
            }

            this.objectContext = objectContext;

            if (this.pushedContext == true)
            {
                throw new InvalidOperationException("can only push one context per scope");
            }

            this.pushedContext = true;
            //dont need to lock since it is thread static
            innerList.AddLast(objectContext);

            return retVal;
        }

        private bool poppeddContext;
        protected void PopContext()
        {
            if (this.poppeddContext == true ||
                this.pushedContext == false)
            {
                return;
            }

            this.poppeddContext = true;

            var last = innerList.Last;
            //dont need to lock since it is thread static
            if (last == null)
            {
                this.objectContext.Log.Error("error popping context 0");
                return;
            }

            if (last.Value == this.objectContext)
            {
                innerList.RemoveLast();
                return;
            }

            this.objectContext.Log.Error("error popping context, not in last position 1");

            bool found = false;
            var current = last;
            while (current != null)
            {
                if (current.Value == this.objectContext)
                {
                    found = true;
                    break;
                }

                current = current.Previous;
            }

            if (found == false)
            {
                this.objectContext.Log.Error("error popping context, not in last position, could not find 2");
                return;
            }

            bool isContextNode = false;
            while (last != null && isContextNode == false)
            {
                isContextNode = last.Value == this.objectContext;
                innerList.RemoveLast();

                last = innerList.Last;
            }
        }

        public static IObjectContext CurrentObjectContext
        {
            get
            {
                var last = innerList.Last; //dont need to lock since it is thread static

                if (last == null)
                {
                    return null;
                }

                return last.Value;
            }
        }

        public IObjectContext GetThreadCurrentObjectContext()
        {
            return ObjectContextCreationScope.CurrentObjectContext;
        }

        public void Dispose()
        {
            if (this.IsDisposed == true)
            {
                return;
            }

            this.IsDisposed = true;
            if (this.Disposing != null)
            {
                this.Disposing(this, EventArgs.Empty);
            }

            this.PopContext();
        }

        public event EventHandler Disposing;

        public bool IsDisposed
        {
            get;
            protected set;
        }
    }
}
