using System;
using System.Collections.Generic;
using System.Linq;
using Uorsy.Data;

namespace SearchV2.Api.Uorsy
{
    public class FilterQuery
    {
        public Filter<double>? Mw { get; set; }
        public Filter<double>? Logp { get; set; }
        public Filter<int>? Hba { get; set; }
        public Filter<int>? Hbd { get; set; }
        public Filter<int>? Rotb { get; set; }
        public Filter<double>? Tpsa { get; set; }
        public Filter<double>? Fsp3 { get; set; }
        public Filter<int>? Hac { get; set; }

        public struct Filter<T> where T : struct, IComparable<T>
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
    }
}