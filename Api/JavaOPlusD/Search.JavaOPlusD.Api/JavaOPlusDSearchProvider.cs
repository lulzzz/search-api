using System.Collections.Generic;
using Search.Abstractions;
using System.Net.Http;
using System;
using Newtonsoft.Json;
using System.Web;

namespace Search.JavaOPlusD.Api
{
    public sealed class JavaOPlusDSearchProvider : ISearchProvider, IDisposable
    {
        readonly HttpClient _httpClient;

        public JavaOPlusDSearchProvider(string serviceUrl)
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(serviceUrl);
        }

        public void Dispose()
        {
            if (_httpClient != null)
            {
                _httpClient.Dispose();
            }
        }
        
#warning filters are not supported for now
        public IEnumerable<MoleculeData> Find(SearchQuery searchQuery, FilterQuery filters, int skip, int take)
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

            

#warning shoud be async
            var requestTask = _httpClient.GetStringAsync($"{endpoint}?smiles={HttpUtility.UrlEncode(searchQuery.SearchText)}&skip={skip}&take={take}");
            requestTask.Wait();

            var result = JsonConvert.DeserializeObject<IEnumerable<MoleculeData>>(requestTask.Result);

            return result;
        }

        public MoleculeData Item(string id)
        {
            throw new System.NotImplementedException();
        }
    }
}
