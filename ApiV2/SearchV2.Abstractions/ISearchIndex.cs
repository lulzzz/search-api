using System.Collections.Generic;
using System.Threading.Tasks;

namespace SearchV2.Abstractions
{
    public interface ISearchIndex
    {
        Task Add(IEnumerable<SearchIndexItem> items);
        Task Remove(IEnumerable<string> refs);
    }

    public class SearchIndexItem : IWithReference<string>
    {
        public string Ref { get; set; }
        public string Smiles { get; set; }
    }
}
