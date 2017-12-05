using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Search.Abstractions
{
    public interface IBatchedSearchResult<TId>
    {
        Task<TId[]> Next(int batchSize);
    }
}
