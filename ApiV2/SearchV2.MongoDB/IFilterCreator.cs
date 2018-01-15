using MongoDB.Driver;

namespace SearchV2.MongoDB
{
    public interface IFilterCreator<TFilter, TData>
    {
        FilterDefinition<TData> Create(TFilter filters);
    }
}
