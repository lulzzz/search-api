#pragma warning disable IDE1006 // Naming Styles

namespace Search.PostgresRDKit.Tables
{
    class mols

    {
        public int id { get; set; }
        public string mol { get; set; }
        public byte[] fp { get; set; }
    }
}
#pragma warning restore IDE1006 // Naming Styles
