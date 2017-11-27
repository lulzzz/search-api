using Microsoft.AspNetCore.Mvc;
using Search.Abstractions;
using System.Linq;

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
        public SearchResult Search(SearchRequest request)
        {
            var mols = _searchProvider
                .Find(new SearchQuery { SearchText = request.Text, Type = request.Type }, request.Filters, skip: (request.PageNumber.Value - 1) * request.PageSize.Value, take: request.PageSize.Value)
                .ToArray();

            return new SearchResult { Molecules = mols };
        }
        
        [HttpGet]
        [Route("{id}")]
        public MoleculeData One([FromRoute]string id)
        {
            return _searchProvider.Item(id);
        }
    }
}
