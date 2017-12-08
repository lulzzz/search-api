namespace Search.Abstractions
{
#warning refactor, must not be here
    public struct Filter<T> where T : struct
    {
        public T? Min { get; set; }
        public T? Max { get; set; }
    }
}
