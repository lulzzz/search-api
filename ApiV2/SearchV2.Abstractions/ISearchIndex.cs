using System.Collections.Generic;
using System.Threading.Tasks;

namespace SearchV2.Abstractions
{
    public interface ISearchIndex
    {
        Task Add(IEnumerable<ISearchIndexItem> items);
        Task Remove(IEnumerable<string> refs);
    }

    public class SearchIndexItem : ISearchIndexItem
    {
        public string Ref { get; set; }
        public string Smiles { get; set; }
    }

    public interface ISearchIndexItem : IWithReference<string>
    {
        string Smiles { get; }
    }
}
