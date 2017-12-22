using SearchV2.Abstractions;
using System;

namespace SearchV2.RDKit
{
    public static class RDKitSearchService
    {
        public static ISearchService<string, string, RDKitSubstructureSearchResult> Substructure(string connectionString, int hitLimit)
        {
            return new RDKitSimpleSearchService<RDKitSubstructureSearchResult>(connectionString, hitLimit, $"{nameof(Ms)}.{nameof(Ms.Mol)}@>mol_from_smiles(@SearchText::cstring)");
        }

        public static ISearchService<string, string, RDKitSuperstructureSearchResult> Superstructure(string connectionString, int hitLimit)
        {
            return new RDKitSimpleSearchService<RDKitSuperstructureSearchResult>(connectionString, hitLimit, $"{nameof(Ms)}.{nameof(Ms.Mol)}<@mol_from_smiles(@SearchText::cstring)");
        }

        public static ISearchService<string, RDKitSimilaritySearchRequest, RDKitSimilaritySearchResult> Similar(string connectionString, int hitLimit)
        {
            return new RDKitSimilaritySearchService(connectionString, hitLimit);
        }
    }
}
