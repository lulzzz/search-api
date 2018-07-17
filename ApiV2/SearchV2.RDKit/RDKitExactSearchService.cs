using Npgsql;
using SearchV2.Abstractions;
using SearchV2.Generics;
using System.Data;
using System.Threading.Tasks;

namespace SearchV2.RDKit
{
    using static CommonSqlMethods;

    class RDKitExactSearchService : ISearchComponent<string, RDKitSimpleSearchResult>
    {
        readonly string _connectionString;

        readonly static string selectFromClause =
            $"SELECT {nameof(Mr.Ref)} " +
            $"FROM (SELECT {nameof(Mr.Ref)}, fp FROM {nameof(Mr)} ORDER BY morganbv_fp(mol_from_smiles(@SearchText::cstring))<%>fp LIMIT 1) AS sub " +
            $"WHERE morganbv_fp(mol_from_smiles(@SearchText::cstring))=sub.fp";

        public RDKitExactSearchService(string connectionString)
        {
            _connectionString = connectionString;
        }

        Task<ISearchResult<RDKitSimpleSearchResult>> ISearchComponent<string, RDKitSimpleSearchResult>.FindAsync(string query, int fastFetchCount)
        => Task.FromResult<ISearchResult<RDKitSimpleSearchResult>>(new AsyncResult<RDKitSimpleSearchResult>(
                pushStatus =>
                    WithConnection(_connectionString, async con =>
                    {
                        var searchCommand = con.CreateCommand();
                        searchCommand.CommandText = selectFromClause;
                        searchCommand.Parameters.Add(new NpgsqlParameter("@SearchText", query));

                        using (var fastReader = await searchCommand.ExecuteReaderAsync())
                        {
                            while (await fastReader.ReadAsync())
                            {
                                var nr = new RDKitSimpleSearchResult
                                {
                                    Ref = (string)fastReader[nameof(Mr.Ref)]
                                };
                                pushStatus(null, new[] { nr });
                            }
                        }
                    })
                )
            );
    }
}