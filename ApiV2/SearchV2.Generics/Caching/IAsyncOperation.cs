namespace SearchV2.Generics
{
    public interface IAsyncOperation
    {
        AsyncOperationStatus Status { get; }
    }

    public enum AsyncOperationStatus { Running, Finished, Faulted }
}
