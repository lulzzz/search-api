using SearchV2.Abstractions;

namespace SearchV2.RDKit
{
    public static class RDKitSearchService
    {
        public static ISearchComponent<string, RDKitSimpleSearchResult> Substructure(string connectionString, int hitLimit)
        {
            return new RDKitSimpleSearchService(connectionString, hitLimit, $"mol@>mol_from_smiles(@SearchText::cstring)");
        }

        public static ISearchComponent<string, RDKitSimpleSearchResult> Superstructure(string connectionString, int hitLimit)
        {
            return new RDKitSimpleSearchService(connectionString, hitLimit, $"mol<@mol_from_smiles(@SearchText::cstring)");
        }

        public static ISearchComponent<RDKitSimilaritySearchRequest, RDKitSimilaritySearchResult> Similar(string connectionString, int hitLimit)
        {
            return new RDKitSimilaritySearchService(connectionString, hitLimit);
        }

        public static ISearchComponent<string, RDKitSimpleSearchResult> Exact(string connectionString)
        {
            return new RDKitExactSearchService(connectionString);
        }
    }
}
