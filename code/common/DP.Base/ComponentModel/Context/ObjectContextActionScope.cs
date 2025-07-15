using System;
using System.Collections.Generic;
using DP.Base.Contracts;

namespace DP.Base.Context
{
    public class ObjectContextActionScope : IObjectContextScope
    {
#pragma warning disable SA1309 // Field names must not begin with underscore
#pragma warning disable SA1300 // Element must begin with upper-case letter
        [ThreadStatic]
        private static LinkedList<ObjectContextAction> _innerList;
        private static LinkedList<ObjectContextAction> innerList
        {
            get
            {
                if (_innerList == null)
                {
                    _innerList = new LinkedList<ObjectContextAction>();
                }

                return _innerList;
            }
        }
#pragma warning restore SA1300 // Element must begin with upper-case letter
#pragma warning restore SA1309 // Field names must not begin with underscore

        private ObjectContextAction objectContext;
        public ObjectContextActionScope(ObjectContextAction objectContext)
        {
            this.objectContext = objectContext;
            this.PushContext(objectContext);
        }

        private bool pushedContext;
        protected void PushContext(ObjectContextAction context)
        {
            if (this.pushedContext == true)
            {
                throw new InvalidOperationException("can only push one context per scope");
            }

            this.pushedContext = true;
            //dont need to lock since it is thread static
            innerList.AddLast(context);
        }

        private bool poppeddContext;
        protected void PopContext()
        {
            if (this.poppeddContext == true)
            {
                return;
            }

            this.poppeddContext = true;

            try
            {
                var last = innerList.Last;
                //dont need to lock since it is thread static
                if (last == null)
                {
                    //this.objectContext.Log.Error("error popping context 0");
                    //return;
                    throw new Exception("innerList.Last is null");
                }

                if (last.Value == this.objectContext)
                {
                    innerList.RemoveLast();
                    return;
                }

                //this.objectContext.Log.Error("error popping context, not in last position 1");
                try
                {
                    throw new Exception("Not in last position");
                }
                catch (Exception ex)
                {
                    this.objectContext.Log.Error("Error popping context", ex);
                }

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
                    //this.objectContext.Log.Error("error popping context, not in last position, could not find 2");
                    //return;
                    throw new Exception("Not in last position, could not find match");
                }

                bool isContextNode = false;
                while (last != null && isContextNode == false)
                {
                    isContextNode = last.Value == this.objectContext;
                    innerList.RemoveLast();

                    last = innerList.Last;
                }
            }
            catch (Exception ex)
            {
                this.objectContext.Log.Error("Error popping context", ex);
            }
        }

        public static ObjectContextAction CurrentObjectContext
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
            return ObjectContextActionScope.CurrentObjectContext;
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
