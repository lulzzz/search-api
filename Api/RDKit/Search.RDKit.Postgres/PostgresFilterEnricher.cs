﻿using Npgsql;
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


        readonly static string fieldsClause =
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
            $"mr.{nameof(molecules_raw.hac)} ";

        readonly static string selectFromClause = $"SELECT {fieldsClause} FROM {nameof(molecules_raw)} mr JOIN unnest(@IDs) ids on ids=mr.idnumber";

        async Task<IEnumerable<MoleculeData>> IFilterEnricher<string, FilterQuery, MoleculeData>.FilterAndEnrich(IEnumerable<string> ids, FilterQuery filters)
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
                command.Parameters.Add(new NpgsqlParameter("@IDs", ids));

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

        readonly static string selectOneQuery = $"SELECT {fieldsClause} FROM {nameof(molecules_raw)} mr WHERE mr.{nameof(molecules_raw.idnumber)}=@{nameof(molecules_raw.idnumber)}";

        async Task<MoleculeData> IFilterEnricher<string, FilterQuery, MoleculeData>.One(string id)
        {
            using (var con = new NpgsqlConnection(_connectionString))
            {
                var command = con.CreateCommand();
                command.CommandText = selectOneQuery;

                command.Parameters.Add(new NpgsqlParameter($"@{nameof(molecules_raw.idnumber)}", id));

                await con.OpenAsync();
                var reader = await command.ExecuteReaderAsync();

                if (reader.Read())
                {
                    return new MoleculeData
                    {
                        IdNumber = id,
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
                    };
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
