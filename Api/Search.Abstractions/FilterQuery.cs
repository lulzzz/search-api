namespace Search.Abstractions
{
    public class FilterQuery
    {
        public Filter<double>? Mw { get; set; }
        public Filter<double>? Logp { get; set; }
        public Filter<int>? Hba { get; set; }
        public Filter<int>? Hbd { get; set; }
        public Filter<int>? Rotb { get; set; }
        public Filter<double>? Tpsa { get; set; }
        public Filter<double>? Fsp3 { get; set; }
        public Filter<int>? Hac { get; set; }

        public struct Filter<T> where T : struct
        {
            public T? Min { get; set; }
            public T? Max { get; set; }
        }
    }
}
