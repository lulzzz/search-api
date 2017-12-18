namespace SearchV2.Abstractions
{
    public interface IWithReference<TId>
    {
        TId Ref { get; }
    }
}
