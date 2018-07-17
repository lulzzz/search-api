using MoreLinq;
using Npgsql;
using SearchV2.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SearchV2.RDKit
{
    using static CommonSqlMethods;
    public class RDKitIndex : ISearchIndex
    {
        readonly string _connectionString;

        private RDKitIndex(string connectionString)
        {
            _connectionString = connectionString;
        }

        public static string CreateConnectionString(string address, string dbName) => $"User ID=postgres;Host={address};Port=5432;Database={dbName};Pooling=true;";

        public static async Task<ISearchIndex> OpenOrInit(string address, string dbName)
        {
            var connectionStringPreformatted = $"User ID=postgres;Host={address};Port=5432;Database={{0}};Pooling=true;";

            var dbExists = false;

            await WithConnection(string.Format(connectionStringPreformatted, "postgres"), async c =>
            {
                dbExists = await c.ExecuteCommandScalar("SELECT 1 FROM pg_database WHERE datname='temp'") != null;
                if (!dbExists)
                {
                    await c.ExecuteCommandNonQuery("CREATE DATABASE {dbName}");
                }
            });

            var connectionString = string.Format(connectionStringPreformatted, dbName);

            if (!dbExists)
            {
                await WithConnection(connectionString, async connection =>
                {
                    await connection.ExecuteCommandNonQuery("CREATE EXTENSION rdkit");
                    await connection.ExecuteCommandNonQuery("CREATE TABLE mr (id SERIAL primary key, ref text, smiles text UNIQUE, mol mol UNIQUE, fp bfp)");
                    await connection.ExecuteCommandNonQuery("CREATE INDEX ms_fp_idx ON mr USING gist(fp)");
                    await connection.ExecuteCommandNonQuery("CREATE INDEX ms_mol_idx ON mr USING gist(mol)");
                });
            }

            return new RDKitIndex(connectionString);
        }


        Task ISearchIndex.Add(IEnumerable<ISearchIndexItem> items)
            => WithConnection(_connectionString, c
                => c.RunTransactionAndCommit(async t
                    =>
                    {
                        await t.ExecuteCommandNonQuery("CREATE LOCAL TEMP TABLE IF NOT EXISTS mols_temp (ref text, smiles text) ON COMMIT DELETE ROWS");
                        foreach (var batch in items.Batch(100))
                        {
                            var cmdText = new StringBuilder("INSERT INTO mols_temp (ref, smiles) VALUES ");
                            var counter = 0;
                            var parameters = new List<NpgsqlParameter>();
                            foreach (var item in batch)
                            {
                                parameters.Add(new NpgsqlParameter($"@v{counter}", item.Ref));
                                parameters.Add(new NpgsqlParameter($"@v{counter + 1}", item.Smiles));
                                cmdText.Append($"(@v{counter}, @{counter + 1}),");
                                counter += 2;
                            }
                            cmdText.Remove(cmdText.Length - 1, 1).Append(';');
                            await t.ExecuteCommandNonQuery(cmdText.ToString(), parameters.ToArray());
                        }
                        await t.ExecuteCommandNonQuery(
@"INSERT INTO mr (ref, smiles, mol, fp) 
SELECT ref, smiles, mol, morganbv_fp(mol) fp 
FROM (SELECT ref, smiles, mol_from_smiles(smiles::cstring) mol FROM mols_temp)
ON CONFLICT DO NOTHING");
                    })
            );

        Task ISearchIndex.Remove(IEnumerable<string> refs)
            => WithConnection(_connectionString, c
                => c.RunTransactionAndCommit(async t => {
                    foreach (var item in refs)
                    {
                        await t.ExecuteCommandNonQuery($"DELETE FROM mr WHERE ref = @ref", new NpgsqlParameter("@ref", item));
                    }
                })
            );
    }
}
