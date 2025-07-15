//using DP.Base.Contracts;
//using DP.Base.Utilities;

//namespace DP.Base.Context
//{
//    public class CurrentContextFinder : IContextFinder
//    {
//        public IContext GetCurrentContext()
//        {
//            var context = ObjectContextActionScope.CurrentObjectContext;

//            if (context != null)
//            {
//                return context;
//            }

//            return ApplicationEx.Instance.GlobalContext;
//        }
//    }
//}
