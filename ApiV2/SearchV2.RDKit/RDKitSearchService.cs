using SearchV2.Abstractions;

namespace SearchV2.RDKit
{
    public static class RDKitSearchService
    {
        public static ISearchService<string, string, RDKitSimpleSearchResult> Substructure(string connectionString, int hitLimit)
        {
            return new RDKitSimpleSearchService(connectionString, hitLimit, $"{nameof(Ms)}.{nameof(Ms.Mol)}@>mol_from_smiles(@SearchText::cstring)");
        }

        public static ISearchService<string, string, RDKitSimpleSearchResult> Superstructure(string connectionString, int hitLimit)
        {
            return new RDKitSimpleSearchService(connectionString, hitLimit, $"{nameof(Ms)}.{nameof(Ms.Mol)}<@mol_from_smiles(@SearchText::cstring)");
        }

        public static ISearchService<string, RDKitSimilaritySearchRequest, RDKitSimilaritySearchResult> Similar(string connectionString, int hitLimit)
        {
            return new RDKitSimilaritySearchService(connectionString, hitLimit);
        }
    }
}
