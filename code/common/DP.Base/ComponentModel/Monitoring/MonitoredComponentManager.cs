using System.Collections.Generic;
using DP.Base.Collections;
using DP.Base.Contracts;

namespace DP.Base.Monitoring
{
    public class MonitoredComponentManager
    {
        private static volatile MonitoredComponentManager instance;
        public static MonitoredComponentManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (typeof(MonitoredComponentManager))
                    {
                        if (instance == null)
                        {
                            instance = new MonitoredComponentManager();
                        }
                    }
                }

                return instance;
            }
        }

        protected MonitoredComponentManager()
        {
            this.componentList = new ConcurrentWeakReferenceCollection<IReflectiveMonitor>();
        }

        private ConcurrentWeakReferenceCollection<IReflectiveMonitor> componentList;

        public void AddComponent(IReflectiveMonitor component)
        {
            this.componentList.Add(component);
        }

        public void RemoveComponent(IReflectiveMonitor component)
        {
            this.componentList.Remove(component);
        }

        /// <summary>
        /// returns a local list of the currently available components
        /// </summary>
        /// <returns></returns>
        public List<IReflectiveMonitor> GetComponentList()
        {
            return this.componentList.GetLocalList();
        }

        public IReflectiveMonitor GetComponentByName(string name)
        {
            return this.componentList.GetItem(a => a.Name == name);
        }

        private long nextId = 1L;
        public long GetNextId()
        {
            return System.Threading.Interlocked.Increment(ref this.nextId);
        }
    }
}
