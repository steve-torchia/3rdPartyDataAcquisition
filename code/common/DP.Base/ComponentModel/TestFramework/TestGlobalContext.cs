//using System;
//using DP.Base.Context;
//using DP.Base.Contracts;

//namespace DP.Base.TestFramework
//{
//    public class TestGlobalContext : GlobalContext
//    {
//        public TestGlobalContext()
//            : base()
//        {
//        }
//    }

//    public class TestContextContainer : IContextContainer
//    {
//        public IContext Context
//        {
//            get;
//            set;
//        }

//        public void InitializeAfterCreate()
//        {
//        }

//        public bool IsDisposed
//        {
//            get;
//            set;
//        }

//        public event EventHandler Disposing;

//        public void Dispose()
//        {
//            if (this.IsDisposed)
//            {
//                return;
//            }

//            this.IsDisposed = true;

//            if (this.Disposing != null)
//            {
//                this.Disposing(this, EventArgs.Empty);
//            }
//        }
//    }
//}
