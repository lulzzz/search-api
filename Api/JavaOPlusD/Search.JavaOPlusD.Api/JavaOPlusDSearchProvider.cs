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

#warning should be reflection
        static string BuildFilters(FilterQuery filters)
        {
            var conditions = new List<string>(8);

            if (filters.Mw.HasValue)
            {
                var val = filters.Mw.Value;
                if (val.Min.HasValue)
                {
                    conditions.Add($"mw>={val.Min.Value}");
                }
                if (val.Max.HasValue)
                {
                    conditions.Add($"mw<={val.Max.Value}");
                }
            }

            if (filters.Logp.HasValue)
            {
                var val = filters.Logp.Value;
                if (val.Min.HasValue)
                {
                    conditions.Add($"logp>={val.Min.Value}");
                }
                if (val.Max.HasValue)
                {
                    conditions.Add($"logp<={val.Max.Value}");
                }
            }

            if (filters.Hba.HasValue)
            {
                var val = filters.Hba.Value;
                if (val.Min.HasValue)
                {
                    conditions.Add($"hba>={val.Min.Value}");
                }
                if (val.Max.HasValue)
                {
                    conditions.Add($"hba<={val.Max.Value}");
                }
            }

            if (filters.Hbd.HasValue)
            {
                var val = filters.Hbd.Value;
                if (val.Min.HasValue)
                {
                    conditions.Add($"hbd>={val.Min.Value}");
                }
                if (val.Max.HasValue)
                {
                    conditions.Add($"hbd<={val.Max.Value}");
                }
            }

            if (filters.Rotb.HasValue)
            {
                var val = filters.Rotb.Value;
                if (val.Min.HasValue)
                {
                    conditions.Add($"rotb>={val.Min.Value}");
                }
                if (val.Max.HasValue)
                {
                    conditions.Add($"rotb<={val.Max.Value}");
                }
            }

            if (filters.Tpsa.HasValue)
            {
                var val = filters.Tpsa.Value;
                if (val.Min.HasValue)
                {
                    conditions.Add($"tpsa>={val.Min.Value}");
                }
                if (val.Max.HasValue)
                {
                    conditions.Add($"tpsa<={val.Max.Value}");
                }
            }

            if (filters.Fsp3.HasValue)
            {
                var val = filters.Fsp3.Value;
                if (val.Min.HasValue)
                {
                    conditions.Add($"fsp3>={val.Min.Value}");
                }
                if (val.Max.HasValue)
                {
                    conditions.Add($"fsp3<={val.Max.Value}");
                }
            }

            if (filters.Hac.HasValue)
            {
                var val = filters.Mw.Value;
                if (val.Min.HasValue)
                {
                    conditions.Add($"hac>={val.Min.Value}");
                }
                if (val.Max.HasValue)
                {
                    conditions.Add($"hac<={val.Max.Value}");
                }
            }

            return string.Join(" AND ", conditions);
        }
        
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
            var url = $"{endpoint}?smiles={HttpUtility.UrlEncode(searchQuery.SearchText)}&skip={skip}&take={take}";
            if (filters != null) {
                var filterQuery = BuildFilters(filters);
                if(filterQuery != "")
                {
                    url = $"{url}&filter={HttpUtility.UrlEncode(filterQuery)}";
                }
            }
            var requestTask = _httpClient.GetStringAsync(url);
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
