using System;
using System.Collections.Generic;
using System.Text;

namespace Search.Abstractions
{
    public class SearchQuery
    {
        public SearchType Type { get; set; }
        
        public string SearchText { get; set; }

        public override int GetHashCode() => $"{Type}|{SearchText}".GetHashCode();
    }
}
