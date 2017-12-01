using Microsoft.AspNetCore.Mvc;
using Search.Abstractions;
using System.Linq;
using System.Threading.Tasks;

namespace Search.ApiCore
{
    [Route("molecules")]
    [Controller]
    [MoleculesControllerNameConvention]
    public sealed class MoleculesControllerBase<TId, TData> // : ControllerBase
    {
        public MoleculesControllerBase(ICatalog<TId, TData> catalog)
        {
            Catalog = catalog;
        }

        public ICatalog<TId, TData> Catalog { get; }
        
        [HttpPost]
        [Route("search")]
        public async Task<object> Search(SearchRequest request)
        {
            var mols = await Catalog
                .FindAsync(new SearchQuery { SearchText = request.Text, Type = request.Type }, request.Filters, skip: (request.PageNumber.Value - 1) * request.PageSize.Value, take: request.PageSize.Value);

            return new { Molecules = mols.ToList() };
        }
        
        [HttpGet]
        [Route("{id}")]
        public Task<TData> One([FromRoute]TId id) => Catalog.ItemAsync(id);
    }
}
