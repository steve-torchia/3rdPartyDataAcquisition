using System;

namespace DP.Base.Contracts.Security
{
    [Serializable]
    public class UserInformation
    {
        public UserInformation()
        {
        }

        public virtual Guid UserId { get; set; }
        public virtual Guid? IndividualUserGroupId { get; set; }
        public virtual UserGroupInformation IndividualUserGroup { get; set; }

        public virtual string LoginFullName { get; set; }
        public virtual string RootGroupLoginName 
        {
            get
            {
                if (string.IsNullOrEmpty(this.LoginFullName))
                {
                    return string.Empty;
                }

                int lastAt = this.LoginFullName.LastIndexOf("@");
                if (lastAt == -1 || lastAt == this.LoginFullName.Length - 1)
                {
                    return string.Empty;
                }

                return this.LoginFullName.Substring(lastAt + 1);
            }
        }

        public virtual string PrimaryEmail { get; set; }
        public virtual string Alias { get; set; }
        public virtual string UserLoginName 
        {
            get
            {
                if (string.IsNullOrEmpty(this.LoginFullName))
                {
                    return string.Empty;
                }

                int lastAt = this.LoginFullName.LastIndexOf("@");
                if (lastAt == -1 || lastAt == this.LoginFullName.Length - 1)
                {
                    return this.LoginFullName;
                }

                return this.LoginFullName.Substring(0, lastAt);
            }
        }

        public virtual string FriendlyName
        {
            get;
            set;
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", this.UserId, this.LoginFullName);
        }
    }
}
