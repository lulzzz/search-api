using System.Collections.Generic;
using Search.Abstractions;
using System.Net.Http;
using System;
using Newtonsoft.Json;
using System.Web;
using System.Threading.Tasks;
using Search.SqlCommon;

namespace Search.JavaOPlusD
{
    public sealed class JavaOPlusDSearchProvider : ICatalog<string, FilterQuery, MoleculeData>, IDisposable
    {
        readonly HttpClient _httpClient;

        public JavaOPlusDSearchProvider(string serviceUrl)
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(serviceUrl)
            };
        }

        public void Dispose()
        {
            if (_httpClient != null)
            {
                _httpClient.Dispose();
            }
        }
        
        public async Task<CatalogResult<MoleculeData>> FindAsync(SearchQuery searchQuery, FilterQuery filters, int skip, int take)
        {
            string endpoint;
            switch (searchQuery.Type)
            {
                case SearchType.Exact:
                    endpoint = "exact";
                    break;
                case SearchType.Substructure:
                    endpoint = "sub";
                    break;
                case SearchType.Similar:
                    endpoint = "sim";
                    break;
                case SearchType.Superstructure:
                    endpoint = "sup";
                    break;
                case SearchType.Smart:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentException();
            }
            
            var url = $"{endpoint}?smiles={HttpUtility.UrlEncode(searchQuery.SearchText)}&skip={skip}&take={take}";
            if (filters != null) {
                var filterQuery = FilterBuilder.BuildFilters(filters);
                if(filterQuery != "")
                {
                    url = $"{url}&filter={HttpUtility.UrlEncode(filterQuery)}";
                }
            }

            var response = await _httpClient.GetStringAsync(url);

            return new CatalogResult<MoleculeData> { Data = JsonConvert.DeserializeObject<IEnumerable<MoleculeData>>(response) };
        }

        public Task<MoleculeData> ItemAsync(string id)
        {
            throw new System.NotImplementedException();
        }
    }
}
