using Npgsql;
using Search.Abstractions;
using Search.RDKit.Postgres.Tables;
using Search.SqlCommon;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Search.RDKit.Postgres
{
    public class PostgresFilterEnricher : IFilterEnricher<string, FilterQuery, MoleculeData>
    {
        readonly string _connectionString;

        public PostgresFilterEnricher(string connectionString)
        {
            _connectionString = connectionString;
        }

        readonly static string selectFromClause = $"SELECT " +
            $"mr.{nameof(molecules_raw.idnumber)}, " +
            $"mr.{nameof(molecules_raw.smiles)}, " +
            $"mr.{nameof(molecules_raw.name)}, " +
            $"mr.{nameof(molecules_raw.mw)}, " +
            $"mr.{nameof(molecules_raw.logp)}, " +
            $"mr.{nameof(molecules_raw.hba)}, " +
            $"mr.{nameof(molecules_raw.hbd)}, " +
            $"mr.{nameof(molecules_raw.rotb)}, " +
            $"mr.{nameof(molecules_raw.tpsa)}, " +
            $"mr.{nameof(molecules_raw.fsp3)}, " +
            $"mr.{nameof(molecules_raw.hac)} " +
            $"FROM {nameof(molecules_raw)} mr JOIN unnest(@IDs) ids on ids=mr.idnumber";

        public async Task<IEnumerable<MoleculeData>> FilterAndEnrich(IEnumerable<string> ids, FilterQuery filters)
        {
            using (var con = new NpgsqlConnection(_connectionString))
            {
                var command = con.CreateCommand();
                string condition = null;
                if (filters != null)
                {
                    condition = FilterBuilder.BuildFilters(filters.Enumerate());
                }

                command.CommandText = string.IsNullOrEmpty(condition)
                    ? selectFromClause
                    : $"{selectFromClause} WHERE {condition}";

                con.Open();
                var reader = await command.ExecuteReaderAsync();
                var results = new List<MoleculeData>();

                while (reader.Read())
                {
#warning should be reflection
                    results.Add(new MoleculeData
                    {
                        IdNumber = (string)reader[nameof(molecules_raw.idnumber)],
                        Smiles = (string)reader[nameof(molecules_raw.smiles)],
                        Name = (string)reader[nameof(molecules_raw.name)],
                        Mw = (double)reader[nameof(molecules_raw.mw)],
                        Logp = (double)reader[nameof(molecules_raw.logp)],
                        Hba = (int)reader[nameof(molecules_raw.hba)],
                        Hbd = (int)reader[nameof(molecules_raw.hbd)],
                        Rotb = (int)reader[nameof(molecules_raw.rotb)],
                        Tpsa = (double)reader[nameof(molecules_raw.tpsa)],
                        Fsp3 = (double)reader[nameof(molecules_raw.fsp3)],
                        Hac = (int)reader[nameof(molecules_raw.hac)],
                    });
                }

                return results;
            }
        }
    }
}
