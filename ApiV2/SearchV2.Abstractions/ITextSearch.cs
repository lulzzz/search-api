using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SearchV2.Abstractions
{
    public interface ITextSearch<TData>
    {
        Task<IEnumerable<TData>> FindText(string text);
    }
}
