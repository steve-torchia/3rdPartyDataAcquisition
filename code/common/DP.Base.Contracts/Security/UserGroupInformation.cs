using System;

namespace DP.Base.Contracts.Security
{
    [Serializable]
    public class UserGroupInformation
    {
        public virtual Guid Id
        {
            get;
            set;
        }

        public virtual Guid TypeId
        {
            get;
            set;
        }

        public virtual Guid? ParentId
        {
            get;
            set;
        }

        public virtual string FriendlyName
        {
            get;
            set;
        }

        public virtual string Name
        {
            get;
            set;
        }

        public virtual UserGroupInformation Parent
        {
            get;
            set;
        }

        public virtual UserGroupInformation[] Children
        {
            get;
            set;
        }

        public virtual Guid RootId
        {
            get;
            set;
        }

        public virtual UserGroupInformation Root
        {
            get;
            set;
        }

        public override bool Equals(object obj)
        {
            if (obj is UserGroupInformation)
            {
                return this.Id == ((UserGroupInformation)obj).Id;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", this.Id, this.Name);
        }
    }

    public interface IUserGroupInfoContainer
    {
        UserGroupInformation UserGroupInfo { get; }
    }
}
