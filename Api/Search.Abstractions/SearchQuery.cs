namespace Search.Abstractions
{
    public class SearchQuery
    {
        public SearchType Type { get; set; }
        
        public string SearchText { get; set; }
    }
}
