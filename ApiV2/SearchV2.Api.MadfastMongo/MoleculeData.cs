using SearchV2.Abstractions;
using System;

namespace SearchV2.Api.MadfastMongo
{
    public class MoleculeData : IWithReference<String>
    {
        public string Ref { get; set; }

        public string Smiles { get; set; }

        public double Mw { get; set; }
        public double Logp { get; set; }
        public int? Hba { get; set; }
        public int? Hbd { get; set; }
        public int? Rotb { get; set; }
        public double? Tpsa { get; set; }
        public double Fsp3 { get; set; }
        public int Hac { get; set; }
    }
}
