namespace Search.Abstractions
{
    public struct Filter<T> where T : struct
    {
        public T? Min { get; set; }
        public T? Max { get; set; }
    }
}
