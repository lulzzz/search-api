using SearchV2.Abstractions;

namespace SearchV2.Redis
{
    public interface ISerializer<TData> where TData : IWithReference<string>
    {
        byte[] Serialize(TData data);
        TData Deserialize(byte[] raw);
    }
}
