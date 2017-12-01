using Microsoft.AspNetCore.Mvc;
using Search.Abstractions;
using System.Linq;
using System.Threading.Tasks;

namespace Search.ApiCore
{
    [Route("molecules")]
    [Controller]
    [MoleculesControllerNameConvention]
    public sealed class MoleculesControllerBase<TId, TFilterQuery, TData>
    {
        readonly ICatalog<TId, TFilterQuery, TData> _catalog;

        public MoleculesControllerBase(ICatalog<TId, TFilterQuery, TData> catalog)
        {
            _catalog = catalog;
        }

        
        [HttpPost]
        [Route("search")]
        public async Task<object> Search(SearchRequest<TFilterQuery> request)
        {
            var mols = await _catalog
                .FindAsync(new SearchQuery { SearchText = request.Text, Type = request.Type }, request.Filters, skip: (request.PageNumber.Value - 1) * request.PageSize.Value, take: request.PageSize.Value);

            return new { Molecules = mols.Data.ToList() };
        }
        
        [HttpGet]
        [Route("{id}")]
        public Task<TData> One([FromRoute]TId id) => _catalog.ItemAsync(id);
    }
}
