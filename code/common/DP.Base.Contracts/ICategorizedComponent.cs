using System.Collections.Generic;
using DP.Base.Contracts.ComponentModel;

namespace DP.Base.Contracts
{
    public interface ICategorizedComponent : INamedComponent
    {
        List<string> ComponentCategories { get; }
    }
}
