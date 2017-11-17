using Microsoft.AspNetCore.Mvc;
using Search.Abstractions;
using System.ComponentModel.DataAnnotations;

namespace Search.ApiCore
{
    //public class ComparisonDescriptor<T>
    //{
    //    public enum Operator { Eq, Gt, Lt }

    //    public T Value { get; set; }
        
    //    public Operator Type { get; set; }
    //}

    public class SearchRequest
    {
        /// <summary>
        /// Search text that is SMILES for all search types except '<see cref="SearchType.Smart"/>'
        /// </summary>
        [FromQuery, Required]
        public string Text { get; set; }

        [FromQuery]
        public SearchType Type { get; set; } = SearchType.Smart;

        [FromQuery]
        public int? PageSize { get; set; } = 12;

        [FromQuery]
        public int? PageNumber { get; set; } = 1;
    }
}