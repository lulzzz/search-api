using Search.Abstractions;
using System;

namespace Search.RDKit.Postgres
{
    public class CommonQueries
    {
        public static string BuildCondition(SearchQuery searchQuery)
        {
            switch (searchQuery.Type)
            {
#warning Smart is not actually implemented, for now is exact molecule
                case SearchType.Smart:
                case SearchType.Exact:
                    return "ms.mol=mol_from_smiles(@SearchText::cstring)";
                case SearchType.Substructure:
                    return "ms.mol@>mol_from_smiles(@SearchText::cstring)";// ORDER BY tanimoto_sml(morganbv_fp(mol_from_smiles(@SearchText::cstring)), ms.fp) DESC";
                case SearchType.Similar:
#warning subject to query corrections
                    return "morganbv_fp(mol_from_smiles(@SearchText::cstring))%ms.fp";// ORDER BY morganbv_fp(mol_from_smiles(@SearchText::cstring))<%>ms.fp";
                case SearchType.Superstructure:
                    return "ms.mol<@mol_from_smiles(@SearchText::cstring)";// ORDER BY tanimoto_sml(morganbv_fp(mol_from_smiles(@SearchText::cstring)), ms.fp) DESC";
                default:
                    throw new ArgumentException();
            }
        }
    }
}
