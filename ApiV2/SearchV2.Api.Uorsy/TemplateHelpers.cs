using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SearchV2.Api.Uorsy
{
    public class TemplateHelpers
    {
        public static string FormatProducts(int count) => count == 1 ? "product" : "products";
    }
}
