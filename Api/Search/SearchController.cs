using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Search.Abstractions;
using Search.PostgresRDKit.Tables;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Search
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
            var mols = _searchProvider.Find(new SearchQuery { SearchText = request.Text, Type = request.Type }, skip: (request.PageNumber.Value - 1) * request.PageSize.Value, take: request.PageSize.Value)
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
