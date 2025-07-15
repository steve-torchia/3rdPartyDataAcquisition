//using DP.Base.ComponentModel;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using DP.Base.Interfaces.ServiceLocator;
//using DP.Base.Security;
//using DP.Base.ServiceLocator;

//namespace DP.Base.Contracts.ServiceLocator
//{
//  //different service locators for different users
//  //not sure if need to have the commonTypeId GetInstance methods, or if can require a valid key when have multiple of same type
//  //still a WIP
//  public interface IComponentRegistrarProvider 
//  {
//      GetComponentRegistrarEventArgs GetComponentRegistrationInfo(UserGroupInformation userGroupInfo);

//      event DP.Base.ServiceLocator.GetComponentRegistrarEventHandler GetComponentRegistrationInfo_Pre;
//      event DP.Base.ServiceLocator.GetComponentRegistrarEventHandler GetComponentRegistrationInfo_Post;

//  }
//}
