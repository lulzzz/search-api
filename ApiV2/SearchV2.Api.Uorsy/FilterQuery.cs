using SearchV2.Generics;
using System;
using System.Collections.Generic;
using System.Linq;
using Uorsy.Data;

namespace SearchV2.Api.Uorsy
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

        bool IFilterQuery.HasConditions => Enumerate().Any();

        public struct Filter<T> where T : struct, IComparable<T>
        {
            public T? Min { get; set; }
            public T? Max { get; set; }
        }
        
        
#warning should be reflection and should not be here
        public IEnumerable<Func<MoleculeData, bool>> Enumerate()
        {
            if (Mw.HasValue) foreach (var f in BuildFilter(md => md.Mw, Mw.Value)) yield return f;
            if (Logp.HasValue) foreach (var f in BuildFilter(md => md.Logp, Logp.Value)) yield return f;
            if (Hba.HasValue) foreach (var f in BuildFilter(md => md.Hba, Hba.Value)) yield return f;
            if (Hbd.HasValue) foreach (var f in BuildFilter(md => md.Hbd, Hbd.Value)) yield return f;
            if (Rotb.HasValue) foreach (var f in BuildFilter(md => md.Rotb, Rotb.Value)) yield return f;
            if (Tpsa.HasValue) foreach (var f in BuildFilter(md => md.Tpsa, Tpsa.Value)) yield return f;
            if (Fsp3.HasValue) foreach (var f in BuildFilter(md => md.Fsp3, Fsp3.Value)) yield return f;
            if (Hac.HasValue) foreach (var f in BuildFilter(md => md.Hac, Hac.Value)) yield return f;
        }

        public static Func<MoleculeData, bool> CreateFilterDelegate(FilterQuery filters)
        {
            var predicates = filters.Enumerate().ToArray();
            if(predicates.Length == 0)
            {
                return md => true;
            }
            return md =>
            {
                foreach (var p in predicates)
                {
                    if (!p(md)) return false;
                }
                return true;
            };
        }

        IEnumerable<Func<MoleculeData, bool>> BuildFilter<T>(Func<MoleculeData, T> fieldSelector, Filter<T> val) where T : struct, IComparable<T>
        {
            if (val.Min.HasValue && val.Max.HasValue && val.Min.Value.CompareTo(val.Max.Value) == 0)
            {
                var eqVal = val.Min.Value;
                yield return md => fieldSelector(md).CompareTo(eqVal) == 0;
            }
            else
            {
                if (val.Min.HasValue)
                {
                    var minVal = val.Min.Value;
                    yield return md => fieldSelector(md).CompareTo(minVal) >= 0;
                }
                if (val.Max.HasValue)
                {
                    var maxValue = val.Max.Value;
                    yield return md => fieldSelector(md).CompareTo(maxValue) <= 0;
                }
            }
        }
    }
}