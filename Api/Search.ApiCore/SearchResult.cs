using Search.Abstractions;
using System.Collections.Generic;

namespace Search.ApiCore
{
    public class SearchResult
    {
        public IEnumerable<MoleculeData> Molecules { get; set; }
    }
}