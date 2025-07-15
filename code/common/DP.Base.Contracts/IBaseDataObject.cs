using System;

namespace DP.Base.Contracts
{
    public interface IBaseDataObject
    {
        System.Guid Id { get; set; }

        System.Guid CreateUserId { get; set; }

        System.DateTime CreateTime { get; set; }

        System.Guid UpdateUserId { get; set; }

        System.DateTime UpdateTime { get; set; }

        DateTime? DeleteAfter { get; set; }

        byte[] RowVer { get; set; }
    }
}
