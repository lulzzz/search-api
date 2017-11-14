using System;
using System.Collections.Generic;
using System.Text;

namespace Search.PostgresRDKit.Tables
{
    class mols
    {
        public int id { get; set; }
        public string mol { get; set; }
        public byte[] fp { get; set; }
    }
}
