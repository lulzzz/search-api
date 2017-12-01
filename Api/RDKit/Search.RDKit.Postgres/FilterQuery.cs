using Search.SqlCommon;
using System.Collections;
using System.Collections.Generic;

namespace Search.Abstractions
{
    public class FilterQuery : IEnumerable<NamedFilter>
    {
        public Filter<double>? Mw { get; set; }
        public Filter<double>? Logp { get; set; }
        public Filter<int>? Hba { get; set; }
        public Filter<int>? Hbd { get; set; }
        public Filter<int>? Rotb { get; set; }
        public Filter<double>? Tpsa { get; set; }
        public Filter<double>? Fsp3 { get; set; }
        public Filter<int>? Hac { get; set; }

#warning should be reflection and should not be here
        public IEnumerator<NamedFilter> GetEnumerator()
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

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
