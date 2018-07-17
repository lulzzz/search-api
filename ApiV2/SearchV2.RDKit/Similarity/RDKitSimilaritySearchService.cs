using System.Threading.Tasks;
using SearchV2.Abstractions;
using Npgsql;
using SearchV2.Generics;
using System;
using System.Collections.Generic;
using System.Data;

namespace SearchV2.RDKit
{
    class RDKitSimilaritySearchService : ISearchComponent<RDKitSimilaritySearchRequest, RDKitSimilaritySearchResult>
    {
        private string _connectionString;
        private int _hitLimit;

        public RDKitSimilaritySearchService(string connectionString, int hitLimit)
        {
            _connectionString = connectionString;
            _hitLimit = hitLimit;
        }

        readonly static string selectFromClause = $"DECLARE search_cur CURSOR FOR " +
            $"SELECT {nameof(Mr.Ref)}, tanimoto_sml(morganbv_fp(mol_from_smiles(@SearchText::cstring)), fp) AS {nameof(RDKitSimilaritySearchResult.Similarity)} " +
            $"FROM {nameof(Mr)} " +
            "WHERE morganbv_fp(mol_from_smiles(@SearchText::cstring))%fp ORDER BY morganbv_fp(mol_from_smiles(@SearchText::cstring))<%>fp " +
            "LIMIT {0}";

        const string fetchLimitedQuery = "FETCH {0} FROM search_cur";

        const string setThresholdQuery = "SET rdkit.tanimoto_threshold={0}";

        async Task<ISearchResult<RDKitSimilaritySearchResult>> ISearchComponent<RDKitSimilaritySearchRequest, RDKitSimilaritySearchResult>.FindAsync(RDKitSimilaritySearchRequest query, int fastFetchCount)
        {
            var con = new NpgsqlConnection(_connectionString);
            con.Open();
            var t = con.BeginTransaction();

            var setCommand = con.CreateCommand();
            setCommand.CommandText = string.Format(setThresholdQuery, query.SimilarityThreshold ?? 0.3);
            var setTask = setCommand.ExecuteNonQueryAsync();

            var searchCommand = con.CreateCommand();
            searchCommand.CommandText = string.Format(selectFromClause, _hitLimit);
            searchCommand.Parameters.Add(new NpgsqlParameter("@SearchText", query.Query));

            await setTask;
            var searchTask = searchCommand.ExecuteNonQueryAsync();

            return new AsyncResult<RDKitSimilaritySearchResult>(
                pushStatus =>
                {
                    async Task LoadAndUpdate(int fetchCount, int leftToLoad)
                    {
                        await searchTask;
                        fetchCount = Math.Min(leftToLoad, fetchCount);
                        var fetchCommand = t.Connection.CreateCommand();
                        fetchCommand.CommandText = string.Format(fetchLimitedQuery, fetchCount);

                        var results = new List<RDKitSimilaritySearchResult>(fetchCount);

                        using (var fastReader = await fetchCommand.ExecuteReaderAsync())
                        {
                            while (fastReader.Read())
                            {
                                var nr = new RDKitSimilaritySearchResult
                                {
                                    Ref = (string)fastReader[nameof(Mr.Ref)],
                                    Similarity = (double)fastReader[nameof(RDKitSimilaritySearchResult.Similarity)]
                                };
                                results.Add(nr);
                            }
                        }

                        // if loaded all or reached litLimit
                        var continuation = results.Count < fetchCount || leftToLoad == fetchCount
                            ? null
                            : LoadAndUpdate(Math.Max(_hitLimit / 10, fastFetchCount * 10), leftToLoad - fetchCount);

                        pushStatus(continuation, results);

                        if (continuation == null)
                        {
                            var c = t.Connection;
                            if (!t.IsCompleted)
                            {
                                await t.CommitAsync();
                            }
                            if (c.State != ConnectionState.Closed)
                            {
                                c.Dispose();
                            }
                        }
                    }

                    return LoadAndUpdate(fastFetchCount, _hitLimit);
                });
        }
    }
}