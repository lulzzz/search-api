namespace Search.SqlCommon
{
    public struct NamedFilter
    {
        public string Name { get; set; }

        public object Min { get; set; }
        public object Max { get; set; }
    }
}
