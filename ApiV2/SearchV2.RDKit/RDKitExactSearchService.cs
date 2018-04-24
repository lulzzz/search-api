using Npgsql;
using SearchV2.Abstractions;
using SearchV2.Generics;
using System.Data;
using System.Threading.Tasks;

namespace SearchV2.RDKit
{
    class RDKitExactSearchService : ISearchComponent<string, string, RDKitSimpleSearchResult>
    {
        readonly string _connectionString;

        readonly static string selectFromClause =
            $"SELECT {nameof(Mr.Ref)} FROM (SELECT {nameof(Mr)}.{nameof(Mr.Ref)}, ms.fp " +
            $"FROM {nameof(Mr)} JOIN {nameof(Ms)} ON {nameof(Mr)}.{nameof(Mr.Id)}={nameof(Ms)}.{nameof(Ms.Id)} " +
            "ORDER BY morganbv_fp(mol_from_smiles(@SearchText::cstring))<%>ms.fp LIMIT 1) AS sub WHERE morganbv_fp(mol_from_smiles(@SearchText::cstring))=sub.fp";

        public RDKitExactSearchService(string connectionString)
        {
            _connectionString = connectionString;
        }

        Task<ISearchResult<RDKitSimpleSearchResult>> ISearchComponent<string, string, RDKitSimpleSearchResult>.FindAsync(string query, int fastFetchCount)
        => Task.FromResult<ISearchResult<RDKitSimpleSearchResult>>(new AsyncResult<RDKitSimpleSearchResult>(
                async pushStatus =>
                {
                    var con = new NpgsqlConnection(_connectionString);
                    con.Open();

                    var searchCommand = con.CreateCommand();
                    searchCommand.CommandText = selectFromClause;
                    searchCommand.Parameters.Add(new NpgsqlParameter("@SearchText", query));

                    using (var fastReader = await searchCommand.ExecuteReaderAsync())
                    {
                        while (fastReader.Read())
                        {
                            var nr = new RDKitSimpleSearchResult
                            {
                                Ref = (string)fastReader[nameof(Mr.Ref)]
                            };
                            pushStatus(null, new[] { nr });
                        }
                    }

                    if (con.State != ConnectionState.Closed)
                    {
                        con.Dispose();
                    }
                }));
    }
}