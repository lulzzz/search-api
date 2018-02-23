using SearchV2.Abstractions;
using System.Collections.Generic;
using System.IO;

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

        public string InChIKey { get; set; }
        public string Cas { get; set; }
        public string Mfcd { get; set; }

        public static IEnumerable<MoleculeData> ReadFromFile(string path)
        {
            using (var reader = new StreamReader(path))
            {
                reader.ReadLine();
                while (!reader.EndOfStream)
                {
                    yield return FromString(reader.ReadLine());
                }
            }
        }

        static MoleculeData FromString(string s)
        {
            var lineItems = s.Split('\t');

            var mw = double.Parse(lineItems[3]);
            var logp = double.Parse(lineItems[4]);
            var hba = int.Parse(lineItems[5]);
            var hbd = int.Parse(lineItems[6]);
            var rotb = int.Parse(lineItems[7]);
            var tpsa = double.Parse(lineItems[8]);
            var fsp3 = double.Parse(lineItems[9]);
            var hac = int.Parse(lineItems[10]);
            var cas = lineItems[12];
            if(cas == "")
            {
                cas = null;
            }
            var mfcd = lineItems[13];
            if (mfcd == "")
            {
                mfcd = null;
            }

            var md = new MoleculeData
            {
                Smiles = lineItems[0],
                Ref = lineItems[1],
                Name = lineItems[2],
                Mw = mw,
                Logp = logp,
                Hba = hba,
                Hbd = hbd,
                Rotb = rotb,
                Tpsa = tpsa,
                Fsp3 = fsp3,
                Hac = hac,
                InChIKey = lineItems[11],
                Cas = cas,
                Mfcd = mfcd
            };

            return md;
        }
    }
}