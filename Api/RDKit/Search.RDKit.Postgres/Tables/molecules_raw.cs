#pragma warning disable IDE1006 // Naming Styles

namespace Search.PostgresRDKit.Tables
{
    public class molecules_raw
    {
        public int id { get; set; }
        public string smiles { get; set; }
        public string idnumber { get; set; }
        public string name { get; set; }
        public double mw { get; set; }
        public double logp { get; set; }
        public int hba { get; set; }
        public int hbd { get; set; }
        public int rotb { get; set; }
        public double tpsa { get; set; }
        public double fsp3 { get; set; }
        public int hac { get; set; }
    }
}

#pragma warning restore IDE1006 // Naming Styles