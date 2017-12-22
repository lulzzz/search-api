using Npgsql;
using SearchV2.Abstractions;
using SearchV2.Generics;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SearchV2.RDKit
{
    internal interface IWithReferenceInternal
    {
        string Ref { set; }
    }

    internal class RDKitSimpleSearchService<T> : ISearchService<string, string, T> where T : class, IWithReference<string>, IWithReferenceInternal, new()
    {
        readonly string _connectionString;
        readonly int _hitLimit;
        readonly string _whereOrderBy;

        readonly static string selectFromClause = $"DECLARE search_cur CURSOR FOR " +
            $"SELECT {nameof(Mr)}.{nameof(Mr.Ref)} " +
            $"FROM {nameof(Mr)} JOIN {nameof(Ms)} ON {nameof(Mr)}.{nameof(Mr.Id)}={nameof(Ms)}.{nameof(Ms.Id)} " +
            "WHERE {0} " +
            "LIMIT {1}";


        const string fetchLimitedQuery = "FETCH {0} FROM search_cur";

        public RDKitSimpleSearchService(string connectionString, int hitLimit, string whereOrderBy)
        {
            _connectionString = connectionString;
            _hitLimit = hitLimit;
            _whereOrderBy = whereOrderBy;
        }

        async Task<ISearchResult<T>> ISearchService<string, string, T>.FindAsync(string query, int fastFetchCount)
        {
            var con = new NpgsqlConnection(_connectionString);
            con.Open();
            var t = con.BeginTransaction();

            var searchCommand = con.CreateCommand();
            searchCommand.CommandText = string.Format(selectFromClause, _whereOrderBy, _hitLimit);
            searchCommand.Parameters.Add(new NpgsqlParameter("@SearchText", query));
            await searchCommand.ExecuteNonQueryAsync();

            return new AsyncResult<T>(
                pushStatus =>
                {
                    async Task LoadAndUpdate(int fetchCount, int leftToLoad)
                    {
                        fetchCount = Math.Min(leftToLoad, fetchCount);
                        var fetchCommand = t.Connection.CreateCommand();
                        fetchCommand.CommandText = string.Format(fetchLimitedQuery, fetchCount);

                        var results = new List<T>(fetchCount);

                        using (var fastReader = await fetchCommand.ExecuteReaderAsync())
                        {
                            while (fastReader.Read())
                            {
                                var nr = new T();
                                ((IWithReferenceInternal)nr).Ref = (string)fastReader[nameof(Mr.Ref)];
                                results.Add(nr);
                            }
                        }

                        // if loaded all or reached litLimit
                        var continuation = results.Count < fetchCount || leftToLoad == fetchCount
                            ? null
                            : LoadAndUpdate(Math.Max(_hitLimit / 10, fastFetchCount * 10), leftToLoad - fetchCount);

                        pushStatus(continuation, results);
                    }

                    return LoadAndUpdate(fastFetchCount, _hitLimit);
                });
        }

//        public static string BuildCondition(SearchQuery searchQuery)
//        {
//            switch (searchQuery.Type)
//            {
//#warning Smart is not actually implemented, for now is exact molecule
//                case SearchType.Smart:
//                case SearchType.Exact:
//                    return "ms.mol=mol_from_smiles(@SearchText::cstring)";
//                case SearchType.Substructure:
//                    return "ms.mol@>mol_from_smiles(@SearchText::cstring)";// ORDER BY tanimoto_sml(morganbv_fp(mol_from_smiles(@SearchText::cstring)), ms.fp) DESC";
//                case SearchType.Similar:
//#warning subject to query corrections
//                    return "morganbv_fp(mol_from_smiles(@SearchText::cstring))%ms.fp";// ORDER BY morganbv_fp(mol_from_smiles(@SearchText::cstring))<%>ms.fp";
//                case SearchType.Superstructure:
//                    return "ms.mol<@mol_from_smiles(@SearchText::cstring)";// ORDER BY tanimoto_sml(morganbv_fp(mol_from_smiles(@SearchText::cstring)), ms.fp) DESC";
//                default:
//                    throw new ArgumentException();
//            }
//        }
    }
}
