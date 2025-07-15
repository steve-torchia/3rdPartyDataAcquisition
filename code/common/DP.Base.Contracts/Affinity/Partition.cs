using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DP.Base.Contracts
{
    public class Partition
    {
        public Guid Id { get; set; }

        public ServicePartitionKind Kind { get; set; }

        public long MaxKey { get; set; }

        public long MinKey { get; set; }

        public string Endpoint { get; set; }

        public string NodeName { get; set; }
    }

    public enum ServicePartitionKind
    {
        /// <summary>
        /// Indicates the partition kind is invalid.
        /// </summary>
        Invalid = 0,

        /// <summary>
        /// Indicates that the partition is based on string names, and is a System.Fabric.SingletonPartitionInformation
        ///    object, that was originally created via System.Fabric.Description.SingletonPartitionSchemeDescription.
        /// </summary>
        Singleton = 1,

        /// <summary>
        ///    Indicates that the partition is based on Int64 key ranges, and is an System.Fabric.Int64RangePartitionInformation
        ///    object that was originally created via System.Fabric.Description.UniformInt64RangePartitionSchemeDescription.
        /// </summary>          
        Int64Range = 2,

        /// <summary>
        ///    Indicates that the partition is based on string names, and is a System.Fabric.NamedPartitionInformation
        ///     object, that was originally created via System.Fabric.Description.NamedPartitionSchemeDescription.
        /// </summary> 
        Named = 3,
    }
}
