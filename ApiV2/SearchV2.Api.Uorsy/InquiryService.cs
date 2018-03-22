using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SearchV2.Api.Uorsy
{
    public sealed class InquiryRequest
    {
        [Required]
        public string Name { get; set; }

        [Required, RegularExpression(@"^\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}\b$")]
        public string Email { get; set; }

        [Required]
        public string Institution { get; set; }
        public string Comments { get; set; }

        public IDictionary<string, InquiryItem> InquiryItems { get; set; }

        public class InquiryItem
        {
            public int? AmountId { get; set; }
            public string Amount { get; set; }
        }
    }


    public sealed class InquiryService
    {

    }
}
