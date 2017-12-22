using SearchV2.Abstractions;

namespace SearchV2.RDKit
{
    public class Ms
    {
        public int Id { get; set; }
        public byte[] Mol { get; set; }
        public byte[] Fp { get; set; }
    }
}
