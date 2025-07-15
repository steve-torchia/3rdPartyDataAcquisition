using System;
using DP.Base.Contracts;

namespace DP.Base.Data
{
    public class BaseDataObjectHelper
    {
        public static readonly BaseDataObjectHelper Instance = new BaseDataObjectHelper();

        protected BaseDataObjectHelper()
        {
        }

        public void SetBaseProperties(IBaseDataObject entityObject, IObjectContext context)
        {
            var utcNow = context.DateTimeProvider.UtcNow;
            var userGroupId = context.UserGroupInfo.Id;

            this.SetBaseProperties(entityObject, utcNow, userGroupId);
        }

        public void SetBaseProperties(IBaseDataObject entityObject, DateTime utcNow, Guid userGroupId)
        {
            if (entityObject.RowVer == null) //if no rowVer assume insert
            {
                if (entityObject.Id == Guid.Empty)
                {
                    entityObject.Id = Guid.NewGuid();
                }

                if (entityObject.UpdateUserId == Guid.Empty)
                {
                    entityObject.UpdateUserId = userGroupId;
                }

                if (entityObject.CreateUserId == Guid.Empty)
                {
                    entityObject.CreateUserId = userGroupId;
                }

                if (entityObject.CreateTime == DateTime.MinValue)
                {
                    entityObject.CreateTime = utcNow;
                }

                if (entityObject.UpdateTime == DateTime.MinValue)
                {
                    entityObject.UpdateTime = utcNow;
                }
            }
            else
            {
                entityObject.UpdateUserId = userGroupId;
                entityObject.UpdateTime = utcNow;
            }
        }
    }
}
