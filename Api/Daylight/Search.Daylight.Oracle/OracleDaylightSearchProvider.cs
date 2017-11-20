using Search.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Search.Daylight.Oracle
{
    public class OracleDaylightSearchProvider : ISearchProvider
    {
        public IEnumerable<MoleculeData> Find(SearchQuery searchQuery, int skip, int take)
        {
            throw new NotImplementedException();
        }

        public MoleculeData Item(string id)
        {
            throw new NotImplementedException();
        }
    }
}
