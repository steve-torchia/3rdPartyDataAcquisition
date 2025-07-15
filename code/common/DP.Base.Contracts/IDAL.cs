using System.Collections.Generic;
using DP.Base.Contracts.ComponentModel;

namespace DP.Base.Contracts
{
    public interface IDAL : IDALBase, IObjectContextContainer, IInitializeAfterCreate
    {
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

        IList<TDO> Upsert<TDO>(IList<TDO> dataObjects, IObjectContext overrideContext);

        TDO Upsert<TDO>(TDO dataObject, IObjectContext overrideContext);

        IEnumerable<TDO> GetAll<TDO>(IObjectContext overrideContext);

        TDO GetById<TDO>(object id, IObjectContext overrideContext);

        IEnumerable<TDO> GetList<TDO>(object filterObject, IObjectContext overrideContext);
    }
}
