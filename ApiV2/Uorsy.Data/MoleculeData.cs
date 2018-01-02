﻿using SearchV2.Abstractions;

namespace Uorsy.Data
{
    public class MoleculeData : IWithReference<string>
    {
        public string Smiles { get; set; }

        public string Ref { get; set; }
        public string Name { get; set; }

        public double Mw { get; set; }
        public double Logp { get; set; }
        public int Hba { get; set; }
        public int Hbd { get; set; }
        public int Rotb { get; set; }
        public double Tpsa { get; set; }
        public double Fsp3 { get; set; }
        public int Hac { get; set; }
    }
}