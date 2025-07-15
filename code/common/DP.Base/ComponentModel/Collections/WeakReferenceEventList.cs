using System;
using System.Collections.Generic;
using System.Reflection;

namespace DP.Base.Collections
{
    public class WeakReferenceEventList : IEnumerable<WeakReferenceEventList.WeakReferenceEvent>
    {
        private List<WeakReferenceEvent> weakRefEvents = new List<WeakReferenceEvent>();

        private static MethodInfo iWeakReferenceEventTargetInvokeMI;
        private static Type[] invokeTypes;
        static WeakReferenceEventList()
        {
            invokeTypes = new Type[] { typeof(WeakReferenceEventList), typeof(object[]) };
            iWeakReferenceEventTargetInvokeMI = typeof(IWeakReferenceEventTarget).GetMethod("Invoke");
        }

        private const int AllEventTypes = int.MinValue;
        public void Add(Delegate eh)
        {
            this.Add(eh.Method, eh.Target, AllEventTypes);
        }

        public void Add(Delegate eh, int interstedEventTypes)
        {
            this.Add(eh.Method, eh.Target, interstedEventTypes);
        }

        public void Add(MethodInfo method, object target)
        {
            this.Add(method, target, AllEventTypes);
        }

        public void Add(IWeakReferenceEventTarget target)
        {
            this.Add(target, AllEventTypes);
        }

        public void Add(IWeakReferenceEventTarget target, int interstedEventTypes)
        {
            MethodInfo mi = target.GetType().GetMethod("Invoke", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, invokeTypes, null);
            this.Add(mi, target, interstedEventTypes, true);
        }

        public void Add(MethodInfo method, object target, int interstedEventTypes)
        {
            this.Add(method, target, interstedEventTypes, false);
        }

        private void Add(MethodInfo method, object target, int interstedEventTypes, bool targetSubscription)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                string temp = (target == null) ? "undefined" : target.GetType().ToString();
                System.Diagnostics.Debug.Assert(
                    !(targetSubscription == false && (target is IWeakReferenceEventTarget)), 
                    string.Format("Should not use IWeakReferenceEventTarget and delegate subscription on same target:{0}", temp));
            }

            for (int i = 0; i < this.weakRefEvents.Count; i++)
            {
                object tmpTarget = this.weakRefEvents[i].Target;
                if (tmpTarget == null && this.weakRefEvents[i].Method.IsStatic == false)
                {
                    this.weakRefEvents.RemoveAt(i);
                    i--;
                    continue;
                }

                if (tmpTarget == target && method == this.weakRefEvents[i].Method)
                {
                    return; // already there
                }
            }

            WeakReferenceEvent newEvent = new WeakReferenceEvent(method, target, interstedEventTypes, targetSubscription);
            for (int i = 0; i < this.weakRefEvents.Count; i++)
            {
                if (this.weakRefEvents[i].Priority > newEvent.Priority)
                {
                    this.weakRefEvents.Insert(i, newEvent);
                    return;
                }
            }

            this.weakRefEvents.Add(newEvent);
        }

        public void Remove(Delegate eh)
        {
            this.Remove(eh.Method, eh.Target);
        }

        public void Remove(IWeakReferenceEventTarget target)
        {
            MethodInfo mi = target.GetType().GetMethod("Invoke", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, invokeTypes, null);
            this.Remove(mi, target);
        }

        public void Remove(MethodInfo method, object target)
        {
            for (int i = 0; i < this.weakRefEvents.Count; i++)
            {
                object tmpTarget = this.weakRefEvents[i].Target;
                if (tmpTarget == null && this.weakRefEvents[i].Method.IsStatic == false)
                {
                    this.weakRefEvents.RemoveAt(i);
                    i--;
                    continue;
                }

                if (tmpTarget == target && method == this.weakRefEvents[i].Method)
                {
                    this.weakRefEvents.RemoveAt(i);
                    return;
                }
            }
        }

        public void CleanupDeadEvents()
        {
            for (int i = 0; i < this.weakRefEvents.Count; i++)
            {
                object tmpTarget = this.weakRefEvents[i].Target;
                if (tmpTarget == null && this.weakRefEvents[i].Method.IsStatic == false)
                {
                    this.weakRefEvents.RemoveAt(i);
                    i--;
                }
            }
        }

        /// <summary>
        /// get count without checking if objects are valid
        /// </summary>
        public int RawCount
        {
            get { return this.weakRefEvents.Count; }
        }

        public void FireEvents(object sender, EventArgs ea)
        {
            this.FireEvents(new object[] { sender, ea });
        }

        public void FireEvents(object[] args)
        {
            this.FireEvents(args, 0);
        }

        public void FireEvents(object[] args, int eventTypeInt)
        {
            int cancelEventArgIndex = -1;
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] is System.ComponentModel.CancelEventArgs)
                {
                    cancelEventArgIndex = i;
                    break;
                }
            }

            bool targetValid = true;
            for (int i = 0; i < this.weakRefEvents.Count; i++)
            {
                targetValid = true;

                this.weakRefEvents[i].FireEvent(args, eventTypeInt, this, ref targetValid);
                if (targetValid == false)
                {
                    this.weakRefEvents.RemoveAt(i);
                    i--;
                    continue;
                }

                if (cancelEventArgIndex != -1 && args[cancelEventArgIndex] != null &&
                    ((System.ComponentModel.CancelEventArgs)args[cancelEventArgIndex]).Cancel == true)
                {
                    return;
                }
            }
        }

        public interface IWeakReferenceEventTarget
        {
            object Invoke(WeakReferenceEventList senderList, object[] parameters);
        }

        public class WeakReferenceEvent
        {
            private MethodInfo method;
            public MethodInfo Method
            {
                get { return this.method; }
            }

            private WeakReference wrTarget;
            public object Target
            {
                get
                {
                    return this.wrTarget.Target;
                }
            }

            private EventPriorityAttribute.EventPriority priority = EventPriorityAttribute.EventPriority.Default;
            public EventPriorityAttribute.EventPriority Priority
            {
                get
                {
                    return this.priority;
                }
            }

            public WeakReferenceEvent(Delegate eh)
                : this(eh.Method, eh.Target, AllEventTypes)
            {
            }

            public WeakReferenceEvent(Delegate eh, int interstedEventTypes)
                : this(eh.Method, eh.Target, interstedEventTypes)
            {
            }

            public WeakReferenceEvent(MethodInfo method, object target)
                : this(method, target, AllEventTypes, false)
            {
            }

            public WeakReferenceEvent(MethodInfo method, object target, int interstedEventTypes)
                : this(method, target, interstedEventTypes, false)
            {
            }

            public WeakReferenceEvent(MethodInfo method, object target, int interstedEventTypes, bool isMethodIWeakReferenceEventTarget)
            {
                this.isMethodIWeakReferenceEventTarget = isMethodIWeakReferenceEventTarget;
                this.method = method;
                this.interstedEventTypes = interstedEventTypes;

                this.wrTarget = new WeakReference(target);
                object[] attributes = this.method.GetCustomAttributes(typeof(EventPriorityAttribute), true);
                if (attributes != null && attributes.Length > 0)
                {
                    this.priority = ((EventPriorityAttribute)attributes[0]).Priority;
                }
            }

            private int interstedEventTypes;
            private bool isMethodIWeakReferenceEventTarget;

            public void FireEvent(object[] args, int eventType, WeakReferenceEventList senderList, ref bool targetValid)
            {
                object target = this.Target;
                targetValid = true;
                if (target == null && this.method.IsStatic == false)
                {
                    targetValid = false;
                    return;
                }

                //if have defined interstedEventTypes then check if there is a match
                if (this.interstedEventTypes != AllEventTypes &&
                    ((this.interstedEventTypes & eventType) == 0))
                {
                    return; // does not match
                }

                this.Invoke(target, senderList, args);
            }

            private object Invoke(object target, WeakReferenceEventList senderList, object[] parameters)
            {
                if (this.method.IsStatic == false)
                {
                    if (target == null)
                    {
                        return null;
                    }
                }

                if (this.isMethodIWeakReferenceEventTarget == true)
                {
                    return ((IWeakReferenceEventTarget)target).Invoke(senderList, parameters);
                }

                return Reflection.ReflectionHelper.MethodInvoke(this.method, target, parameters);
            }

            public delegate void SafeInvokeEventHandler(SafeInvokeEventArgs siea);
            public class SafeInvokeEventArgs
            {
                private object target;
                public object Target
                {
                    get { return this.target; }
                }

                private object[] args;
                public object[] Args
                {
                    get { return this.args; }
                }

                private WeakReferenceEventList senderList;
                public WeakReferenceEventList SenderList
                {
                    get { return this.senderList; }
                }

                public SafeInvokeEventArgs(object target, WeakReferenceEventList senderList, object[] args)
                {
                    this.target = target;
                    this.args = args;
                    this.senderList = senderList;
                }
            }

            private SafeInvokeEventHandler safeInvokeDele = null;
            protected SafeInvokeEventHandler SafeInvokeDele
            {
                get
                {
                    if (this.safeInvokeDele == null)
                    {
                        this.safeInvokeDele = new SafeInvokeEventHandler(this.SafeInvoke);
                    }

                    return this.safeInvokeDele;
                }
            }

            private void SafeInvoke(SafeInvokeEventArgs siea)
            {
                this.Invoke(siea.Target, siea.SenderList, siea.Args);
            }
        }

        #region IEnumerable<WeakReferenceEvent> Members

        public IEnumerator<WeakReferenceEventList.WeakReferenceEvent> GetEnumerator()
        {
            return new WeakReferenceEventList.Enumerator(this);
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new WeakReferenceEventList.Enumerator(this);
        }

        #endregion

        [Serializable]
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        public struct Enumerator : IEnumerator<WeakReferenceEvent>, IDisposable, System.Collections.IEnumerator
        {
            private WeakReferenceEventList list;
            private int index;
            private WeakReferenceEvent current;

            internal Enumerator(WeakReferenceEventList list)
            {
                this.list = list;
                this.index = 0;
                this.current = null;
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (this.index < this.list.weakRefEvents.Count)
                {
                    WeakReferenceEvent tmpTarget = this.list.weakRefEvents[this.index];
                    if (tmpTarget == null && this.list.weakRefEvents[this.index].Method.IsStatic == false)
                    {
                        this.list.weakRefEvents.RemoveAt(this.index);
                        return this.MoveNext();
                    }

                    this.current = tmpTarget;
                    this.index++;
                    return true;
                }

                this.index = this.list.weakRefEvents.Count + 1;
                this.current = null;
                return false;
            }

            public WeakReferenceEvent Current
            {
                get
                {
                    return this.current;
                }
            }

            object System.Collections.IEnumerator.Current
            {
                get
                {
                    return this.Current;
                }
            }

            void System.Collections.IEnumerator.Reset()
            {
                this.index = 0;
                this.current = null;
            }
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class EventPriorityAttribute : Attribute
    {
        private EventPriority priority;
        public EventPriorityAttribute(EventPriority priority)
        {
            this.priority = priority;
        }

        public EventPriority Priority
        {
            get
            {
                return this.priority;
            }
        }

        /// <summary>
        /// Priority0 is first Priority16 is last default is Priority8
        /// </summary>
        public enum EventPriority : byte
        {
            Default = 0x80,
            Priority0 = 0x00,
            Priority1 = 0x10,
            Priority2 = 0x20,
            Priority3 = 0x30,
            Priority4 = 0x40,
            Priority5 = 0x50,
            Priority6 = 0x60,
            Priority7 = 0x70,
            Priority8 = 0x80,
            Priority9 = 0x90,
            Priority10 = 0xA0,
            Priority11 = 0xB0,
            Priority12 = 0xC0,
            Priority13 = 0xD0,
            Priority14 = 0xE0,
            Priority15 = 0xF0,
            Priority16 = 0xFF,
        }
    }
}
