using MongoDB.Driver;
using SearchV2.Generics;
using SearchV2.MongoDB;
using System.Collections.Generic;
using System.Linq;

namespace SearchV2.Api.MadfastMongo
{
    public class FilterQuery : IFilterQuery
    {
        public Filter<double>? Mw { get; set; }
        public Filter<double>? Logp { get; set; }
        public Filter<int>? Hba { get; set; }
        public Filter<int>? Hbd { get; set; }
        public Filter<int>? Rotb { get; set; }
        public Filter<double>? Tpsa { get; set; }
        public Filter<double>? Fsp3 { get; set; }
        public Filter<int>? Hac { get; set; }

#warning hacked
        bool IFilterQuery.HasConditions => true;

        public struct Filter<T> where T : struct
        {
            public T? Min { get; set; }
            public T? Max { get; set; }

            public NamedFilter ToNamed(string name)
            => new NamedFilter
            {
                Name = name,
                Max = Max.HasValue ? (object)Max.Value : null,
                Min = Min.HasValue ? (object)Min.Value : null
            };
        }

        public struct NamedFilter
        {
            public string Name { get; set; }

            public object Min { get; set; }
            public object Max { get; set; }
        }

        //bool IFilterQuery.Empty
        //=> !(Mw.HasValue || Logp.HasValue || Hba.HasValue || Hbd.HasValue || Rotb.HasValue || Tpsa.HasValue || Fsp3.HasValue || Hac.HasValue);

#warning should be reflection and should not be here
        public IEnumerable<NamedFilter> Enumerate()
        {
            if (Mw.HasValue) yield return Mw.Value.ToNamed(nameof(Mw));
            if (Logp.HasValue) yield return Logp.Value.ToNamed(nameof(Logp));
            if (Hba.HasValue) yield return Hba.Value.ToNamed(nameof(Hba));
            if (Hbd.HasValue) yield return Hbd.Value.ToNamed(nameof(Hbd));
            if (Rotb.HasValue) yield return Rotb.Value.ToNamed(nameof(Rotb));
            if (Tpsa.HasValue) yield return Tpsa.Value.ToNamed(nameof(Tpsa));
            if (Fsp3.HasValue) yield return Fsp3.Value.ToNamed(nameof(Fsp3));
            if (Hac.HasValue) yield return Hac.Value.ToNamed(nameof(Hac));
        }

        public class Creator : IFilterCreator<FilterQuery, MoleculeData>
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
                else
                {
                    if (val.Min != null)
                    {
                        yield return _builder.Gte(val.Name, val.Min);
                    }
                    if (val.Max != null)
                    {
                        yield return _builder.Lte(val.Name, val.Max);
                    }
                }
            }
        }
    }
}
