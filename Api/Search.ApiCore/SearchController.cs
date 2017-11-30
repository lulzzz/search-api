using Microsoft.AspNetCore.Mvc;
using Search.Abstractions;
using System.Linq;
using System.Threading.Tasks;

namespace Search.ApiCore
{
    [Controller]
    [Route("molecules")]
    public class MoleculesController // : ControllerBase
    {
        readonly ISearchProvider _searchProvider;

        public MoleculesController(ISearchProvider searchProvider)
        {
            _searchProvider = searchProvider;
        }

        [HttpPost]
        [Route("search")]
        public async Task<SearchResult> Search(SearchRequest request)
        {
            var mols = await _searchProvider
                .FindAsync(new SearchQuery { SearchText = request.Text, Type = request.Type }, request.Filters, skip: (request.PageNumber.Value - 1) * request.PageSize.Value, take: request.PageSize.Value);

            return new SearchResult { Molecules = mols.ToList() };
        }
        
        [HttpGet]
        [Route("{id}")]
        public Task<MoleculeData> One([FromRoute]string id) => _searchProvider.ItemAsync(id);
    }
}
