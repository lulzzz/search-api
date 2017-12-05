using Npgsql;
using Search.Abstractions;
using Search.Abstractions.Batching;
using Search.RDKit.Postgres.Tables;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Search.RDKit.Postgres
{
    public class PostgresRDKitBatchSearcher : IBatchSearcher<string>
    {
        readonly string _connectionString;

        public PostgresRDKitBatchSearcher(string connectionString)
        {
            _connectionString = connectionString;
        }

        readonly static string selectFromClause = $"DECLARE search_cur CURSOR FOR SELECT mr.{nameof(molecules_raw.idnumber)} " +
            $"FROM {nameof(molecules_raw)} mr INNER JOIN {nameof(mols)} ms ON mr.{nameof(molecules_raw.id)}=ms.{nameof(mols.id)} " +
            "WHERE {0}";

        

        public async Task<IBatchedSearchResult<string>> FindAsync(SearchQuery searchQuery)
        {
            var con = new NpgsqlConnection(_connectionString);

            con.Open();

            var t = con.BeginTransaction();

            var searchCommand = con.CreateCommand();
            var condition = CommonQueries.BuildCondition(searchQuery);
            searchCommand.CommandText = string.Format(selectFromClause, condition);
            searchCommand.Parameters.Add(new NpgsqlParameter("@SearchText", searchQuery.SearchText));
            await searchCommand.ExecuteNonQueryAsync();

            return new Result(t);
        }

        private class Result : IBatchedSearchResult<string>, IDisposable
        {
            const string fetchLimitedQuery = "FETCH {0} FROM search_cur";

            readonly NpgsqlTransaction _t;
            bool disposed;

            public Result(NpgsqlTransaction t)
            {
                _t = t;
            }

            public async Task<string[]> Next(int batchSize)
            {
                if (disposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                var fetchCommand = _t.Connection.CreateCommand();
                fetchCommand.CommandText = string.Format(fetchLimitedQuery, batchSize);

                var results = new List<string>(batchSize);

                using (var fastReader = await fetchCommand.ExecuteReaderAsync())
                {
                    while (fastReader.Read())
                    {
                        results.Add((string)fastReader[nameof(molecules_raw.idnumber)]);
                    }
                }

                return results.ToArray();
            }

            public void Dispose()
            {
                disposed = true;
                if (!_t.IsCompleted)
                {
                    _t.Commit();
                }
                if(_t.Connection.State != ConnectionState.Closed)
                {
                    _t.Connection.Close();
                }
            }
        }
    }
}
