﻿using Search.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Search.MongoDB
{
    public class GenericCatalog<TId, TFilterQuery, TData> : ICatalog<TId, TFilterQuery, TData>
    {
        readonly ISearchProvider<TId> _search;
        readonly IFilterEnricher<TId, TFilterQuery, TData> _filterEnricher;

        public GenericCatalog(ISearchProvider<TId> search, IFilterEnricher<TId, TFilterQuery, TData> filterEnricher)
        {
            _search = search;
            _filterEnricher = filterEnricher;
        }

        public async Task<CatalogResult<TData>> FindAsync(SearchQuery searchQuery, TFilterQuery filters, int skip, int take)
        {
            var limit = skip + take;
            var result = await _search.FindAsync(searchQuery, limit);
            var buffer = new List<TId>();
            var results = new List<TData>();
            
            foreach (var item in result)
            {
                buffer.Add(item);
                if(buffer.Count == limit)
                {
                    var filtered = (await _filterEnricher.FilterAndEnrich(buffer, filters)).ToList();
                    var filteredCount = filtered.Count;
                    results.AddRange(filtered);
                    if (results.Count >= skip + take)
                    {
                        buffer.Clear();
                        break;
                    }
                    limit = limit * limit / filteredCount - limit;
                }
            }

            if(buffer.Count > 0)
            {
                results.AddRange(await _filterEnricher.FilterAndEnrich(buffer, filters));
            }

            return new CatalogResult<TData> { Data = results.Skip(skip).Take(take).ToArray() };
        }

        public Task<TData> ItemAsync(TId id)
        {
            throw new NotImplementedException();
        }
    }
}
