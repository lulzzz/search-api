using SearchV2.Abstractions;
using System;

namespace SearchV2.Abstractions
{
    public interface ISearchIndexItemWithTime : ISearchIndexItem
    {
        DateTime LastUpdated { get; }
    }
}
