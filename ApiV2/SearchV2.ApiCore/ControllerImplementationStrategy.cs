using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SearchV2.ApiCore
{
    public class ControllerImplementationStrategy<TReturn, TSearchQuery, TFilterQuery>
    {
        public Task<TReturn> FindAsync(TSearchQuery searchQuery, TFilterQuery filters, int skip, int take)
        {
            throw new NotImplementedException();
        }
    }
}
