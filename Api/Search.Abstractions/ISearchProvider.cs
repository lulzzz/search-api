using System;
using System.Collections.Generic;
using System.Text;

namespace Search.Abstractions
{
    public interface ISearchProvider
    {
        MoleculeData Item(string id);
        IEnumerable<MoleculeData> Find(SearchQuery searchQuery, int skip, int take);
    }
}
