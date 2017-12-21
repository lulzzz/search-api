using Npgsql;
using SearchV2.Abstractions;
using SearchV2.Generics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SearchV2.RDKit
{
    using ISubstrSearchService = ISearchService<string, string, RDKitSearchResult>;
    
    public class RDKitSearchResult : IWithReference<string>
    {
        public string Ref { get; set; }
    }

    public class Mr : IWithReference<string>
    {
        public int Id { get; set; }
        public string Ref { get; set; }
    }

    public class Ms
    {
        public int Id { get; set; }
        public byte[] Mol { get; set; }
        public byte[] Fp { get; set; }
    }

    internal class RDKitSubstructureSearchService : ISubstrSearchService
    {
        readonly string _connectionString;
        readonly int _hitLimit;

        readonly static string selectFromClause = $"DECLARE search_cur CURSOR FOR " +
            $"SELECT {nameof(Mr)}.{nameof(Mr.Ref)} " +
            $"FROM {nameof(Mr)} JOIN {nameof(Ms)} ON {nameof(Mr)}.{nameof(Mr.Id)}={nameof(Ms)}.{nameof(Ms.Id)} " +
            $"WHERE {nameof(Ms)}.{nameof(Ms.Mol)}@>mol_from_smiles(@SearchText::cstring)";
        const string fetchLimitedQuery = "FETCH {0} FROM search_cur";

        public RDKitSubstructureSearchService(string connectionString, int hitLimit)
        {
            _connectionString = connectionString;
            _hitLimit = hitLimit;
        }

        async Task<ISearchResult<RDKitSearchResult>> ISubstrSearchService.FindAsync(string query, int fastFetchCount)
        {
            var con = new NpgsqlConnection(_connectionString);
            con.Open();
            var t = con.BeginTransaction();

            var searchCommand = con.CreateCommand();
            searchCommand.CommandText = selectFromClause;
            searchCommand.Parameters.Add(new NpgsqlParameter("@SearchText", query));
            await searchCommand.ExecuteNonQueryAsync();

            return new AsyncResult<RDKitSearchResult>(
                pushStatus =>
                {
                    async Task LoadAndUpdate(int fetchCount, int leftToLoad)
                    {
                        fetchCount = Math.Min(leftToLoad, fetchCount);
                        var fetchCommand = t.Connection.CreateCommand();
                        fetchCommand.CommandText = string.Format(fetchLimitedQuery, fetchCount);

                        var results = new List<RDKitSearchResult>(fetchCount);

                        using (var fastReader = await fetchCommand.ExecuteReaderAsync())
                        {
                            while (fastReader.Read())
                            {
                                results.Add(new RDKitSearchResult { Ref = (string)fastReader[nameof(Mr.Ref)] });
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
    }
}
