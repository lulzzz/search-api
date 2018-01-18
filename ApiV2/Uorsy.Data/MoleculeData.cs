using SearchV2.Abstractions;
using SearchV2.Redis;
using System.Collections.Generic;
using System.IO;
using System.Text;

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

        public static ISerializer<MoleculeData> Serializer { get; } = new MDSerializer();

        class MDSerializer : ISerializer<MoleculeData>
        {
            MoleculeData ISerializer<MoleculeData>.Deserialize(byte[] raw)
            {
                using (var reader = new BinaryReader(new MemoryStream(raw), Encoding.UTF8))
                {
                    return new MoleculeData
                    {
                        Smiles = reader.ReadString(),
                        Ref = reader.ReadString(),
                        Name = reader.ReadString(),
                        Mw = reader.ReadDouble(),
                        Logp = reader.ReadDouble(),
                        Hba = reader.ReadInt32(),
                        Hbd = reader.ReadInt32(),
                        Rotb = reader.ReadInt32(),
                        Tpsa = reader.ReadDouble(),
                        Fsp3 = reader.ReadDouble(),
                        Hac = reader.ReadInt32()
                    };
                }
            }

            byte[] ISerializer<MoleculeData>.Serialize(MoleculeData data)
            {
                var s = new MemoryStream();

                using (var writer = new BinaryWriter(s))
                {
                    writer.Write(data.Smiles);
                    writer.Write(data.Ref);
                    writer.Write(data.Name);
                    writer.Write(data.Mw);
                    writer.Write(data.Logp);
                    writer.Write(data.Hba);
                    writer.Write(data.Hbd);
                    writer.Write(data.Rotb);
                    writer.Write(data.Tpsa);
                    writer.Write(data.Fsp3);
                    writer.Write(data.Hac);
                }

                return s.ToArray();
            }
        }
    }
}