using System.Collections.Generic;
using DP.Base.Contracts.ComponentModel;

namespace DP.Base.Contracts
{
    public interface IDALBase : IDisposableEx
    {
        IEnumerable<IDALRecorder> DALRecorders { get; set; }
        DALRecorderType RecordingType { get; set; }

        //
        //public ret GetDataWithTranslator<Xltor, ret>(Xltor translator, string dataName, out dataVersion)
        //
        //public ret GetGetDataVersionWithTranslator<Xltor, ret>(Xltor translator, string dataName)
        //
        // GetData??
        //GetGetDataVersionWithTranslator
        //get
        //insert
        //update
        //delete
    }
}