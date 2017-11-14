using System;
using System.Collections.Generic;

namespace Search
{
    public class SearchResult
    {
        public Guid Id { get; set; }
        public IEnumerable<MoleculeData> Molecules { get; set; }
    }
}