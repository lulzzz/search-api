using SearchV2.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SearchV2.InMemory
{
    public class InMemoryCatalogDb<TFilterQuery, TData> : ICatalogDb<string, TFilterQuery, TData> where TData : class, IWithReference<string>
    {
        public delegate Func<TData, bool> FilterCreatorDelegate(TFilterQuery filters);

        readonly TData[] _data;
        readonly Dictionary<string, List<int>> _dictionary;
        readonly FilterCreatorDelegate _filterToPredicate;

        public InMemoryCatalogDb(IEnumerable<TData> data, FilterCreatorDelegate filterCreator, params Func<TData, string>[] keySelectors)
        {
            _data = data.ToArray();
            _dictionary = new Dictionary<string, List<int>>(_data.Length);
            for (int i = 0; i < _data.Length; i++)
            {
                foreach (var s in keySelectors)
                {
                    var key = s(_data[i]);
                    if(key != null)
                    {
                        if(_dictionary.TryGetValue(key, out List<int> existing))
                        {
                            _dictionary[key].Add(i);
                        }
                        else
                        {
                            _dictionary[key] = new List<int>(1) { i };
                        }
                    }
                }
            }
            _filterToPredicate = filterCreator;
        }

        static readonly IEnumerable<TData> _empty = Enumerable.Empty<TData>();
        static readonly IEqualityComparer<TData> _comparer = new ByIdComparer();
        private IEnumerable<TData> GetAsync(IEnumerable<string> ids)
            => ids.Distinct().SelectMany(id => _dictionary.TryGetValue(id, out List<int> dataIndexes) ? dataIndexes.Select(ind => _data[ind]) : _empty);

        Task<IEnumerable<TData>> ICatalogDb<string, TFilterQuery, TData>.GetAsync(IEnumerable<string> keys)
            => Task.FromResult(GetAsync(keys));

        Task<IEnumerable<TData>> ICatalogDb<string, TFilterQuery, TData>.GetFilteredAsync(IEnumerable<string> keys, TFilterQuery filters)
        {
            if (filters == null)
            {
                return Task.FromResult(GetAsync(keys));
            }
            var filterFunc = _filterToPredicate(filters);
            return Task.FromResult(
                GetAsync(keys)
                    .Where(d => d != null && filterFunc(d))
                );
        }

        Task<TData> ICatalogDb<string, TFilterQuery, TData>.OneAsync(string id)
            => Task.FromResult(_dictionary.TryGetValue(id, out List<int> dataIndexes) ? _data[dataIndexes.First()] : null);

        class ByIdComparer : IEqualityComparer<TData>
        {
            public bool Equals(TData x, TData y) => x.Ref == y.Ref;
            public int GetHashCode(TData obj) => obj.Ref.GetHashCode();
        }
    }
}
