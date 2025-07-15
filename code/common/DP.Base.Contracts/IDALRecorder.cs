using System;
using DP.Base.Contracts.ComponentModel;

namespace DP.Base.Contracts
{
    public interface IDALRecorder : IDisposableEx
    {
        DALRecorderType RecorderType { get; }

        void Serialize(string objectName, object obj);

        T DeSerialize<T>(string objectName);
    }

    [Flags]
    public enum DALRecorderType
    {
        None = 0x0000,
        Local = 0x0001,
        Remote = 0x0002,

        Play = 0x10000000,
        Record = 0x20000000,
    }
}
