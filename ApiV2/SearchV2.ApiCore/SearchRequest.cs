﻿using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace SearchV2.ApiCore
{
    public class SearchRequest<TSearchQuery, TFilterQuery>
    {
        public class Body
        {
            public TFilterQuery Filters { get; set; }
            public TSearchQuery Search { get; set; }
        }

        [FromQuery]
        public int? PageSize { get; set; } = 12;

        [FromQuery]
        public int? PageNumber { get; set; } = 1;

        [FromBody, Required]
        public Body Query { get; set; }
    }
}