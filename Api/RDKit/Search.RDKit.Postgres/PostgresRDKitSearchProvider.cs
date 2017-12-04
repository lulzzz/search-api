using Npgsql;
using Search.Abstractions;
using Search.RDKit.Postgres.Tables;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections;
using System.Data.Common;

namespace Search.RDKit.Postgres
{
    public class PostgresRDKitSearchProvider : ISearchProvider<string>
    {
        private class SearchResult : ISearchResult<string>
        {
            readonly NpgsqlConnection _c;
            readonly NpgsqlTransaction _t;
            readonly NpgsqlCommand _com;
            readonly List<string> _fastFetched;

            bool enumerated;

            public SearchResult(NpgsqlConnection c, NpgsqlTransaction t, NpgsqlCommand com, List<string> fastFetched)
            {
                _c = c;
                _t = t;
                _com = com;
                _fastFetched = fastFetched;
            }

            public void Dispose()
            {
                _t.Commit();
                _c.Close();
            }

            public IEnumerator<string> GetEnumerator()
            {
#warning reimplement to allow searchresult to keep its data
                if(enumerated)
                {
                    throw new InvalidOperationException();
                }

                enumerated = true;

                foreach (var item in _fastFetched)
                {
                    yield return item;
                }
                foreach (var item in _fetchingReader)
                {
                    yield return item;
                }

                Dispose();
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        readonly string _connectionString;

        public PostgresRDKitSearchProvider(string connectionString)
        {
            _connectionString = connectionString;
        }

        readonly static string selectFromClause = $"DECLARE search_cur CURSOR FOR SELECT mr.{nameof(molecules_raw.idnumber)} " + 
            $"FROM {nameof(molecules_raw)} mr INNER JOIN {nameof(mols)} ms ON mr.{nameof(molecules_raw.id)}=ms.{nameof(mols.id)} " +
            "WHERE {0}";

        readonly static string fetchLimitedQuery = "FETCH {0} FROM search_cur";

        public async Task<ISearchResult<string>> FindAsync(SearchQuery searchQuery, int fastFetchCount)
        {
            var con = new NpgsqlConnection(_connectionString);

            con.Open();

            var t = con.BeginTransaction();

            var searchCommand = con.CreateCommand();
            var condition = CommonQueries.BuildCondition(searchQuery);
            searchCommand.CommandText = string.Format(selectFromClause, condition);
            searchCommand.Parameters.Add(new NpgsqlParameter("@SearchText", searchQuery.SearchText));

            var fastFetchCommand = con.CreateCommand();
            fastFetchCommand.CommandText = string.Format(fetchLimitedQuery, fastFetchCount);

            var fetchAllCommand = con.CreateCommand();
            fetchAllCommand.CommandText = fetchAllQuery;

            await searchCommand.ExecuteNonQueryAsync();

            var results = new List<string>();

            using (var fastReader = await fastFetchCommand.ExecuteReaderAsync())
            {
                while (fastReader.Read())
                {
                    results.Add((string)fastReader[nameof(molecules_raw.idnumber)]);
                }
            }

            fetchAllCommand.ExecuteReaderAsync();

            //var restResults = Read(await fetchAllCommand.ExecuteReaderAsync());

            return new SearchResult(con, t, fetchAllCommand, results);
        }
    }
}
