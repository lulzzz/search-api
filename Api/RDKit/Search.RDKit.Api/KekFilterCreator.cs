using Search.MongoDB;
using Search.RDKit.Postgres;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using Search.SqlCommon;

namespace Search.RDKit.Api
{
    public class KekFilterCreator : IFilterCreator<FilterQuery, MoleculeData>
    {
        readonly FilterDefinitionBuilder<MoleculeData> _builder = Builders<MoleculeData>.Filter;

        FilterDefinition<MoleculeData> IFilterCreator<FilterQuery, MoleculeData>.Create(FilterQuery filters)
        {
            return filters.Enumerate().SelectMany(BuildFilter).Aggregate(_builder.Empty, (acc, item) => acc & item);
        }

        IEnumerable<FilterDefinition<MoleculeData>> BuildFilter(NamedFilter val)
        {
            if (val.Min == val.Max && val.Min != null)
            {
                yield return _builder.Eq(val.Name, val.Min);
            }
            else {
                if (val.Min != null)
                {
                    yield return _builder.Gt(val.Name, val.Min);
                }
                if (val.Max != null)
                {
                    yield return _builder.Lt(val.Name, val.Max);
                }
            }
        }
    }
}
