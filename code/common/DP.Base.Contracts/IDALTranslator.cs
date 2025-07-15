using System.Collections.Generic;

namespace DP.Base.Contracts
{
    public interface IDALTranslator<TDO, TEC>
    {
        IDAL Dal { get; }

        void Initialize(IDAL dal);

        object GetSharedObject(IObjectContext overrideContext);

        TDO ToDataObject(object entityObject, TEC entityContext, IObjectContext overrideContext, object sharedObject);

        object ToEntityObject(TDO dataObject, TEC entityContext, IObjectContext overrideContext, object sharedObject);

        object PrepareForUpsert(TDO dataObject, TEC entityContext, IObjectContext overrideContext, object sharedObject);

        IEnumerable<TDO> GetAll(TEC entityContext, IObjectContext overrideContext, object sharedObject);

        IEnumerable<TDO> GetList(object filterObject, TEC entityContext, IObjectContext overrideContext, object sharedObject);

        TDO GetById(object id, TEC entityContext, IObjectContext overrideContext, object sharedObject);
    }
}
