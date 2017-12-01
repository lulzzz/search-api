using Npgsql;
using Search.Abstractions;
using Search.RDKit.Postgres.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Search.RDKit.Postgres
{
    public class PostgresRDKitSearchProvider : ICatalog<string, MoleculeData>
    {
#warning should be reflection        
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
            $"FROM {nameof(molecules_raw)} mr INNER JOIN {nameof(mols)} ms ON mr.{nameof(molecules_raw.id)}=ms.{nameof(mols.id)} " +
            "WHERE {0} " +
            "LIMIT @Limit OFFSET @Offset";

        readonly string _connectionString;

        public PostgresRDKitSearchProvider(string connectionString)
        {
            _connectionString = connectionString;
        }

        static string BuildCondition(SearchQuery searchQuery)
        {
            switch (searchQuery.Type)
            {
#warning Smart is not actually implemented, for now is exact molecule
                case SearchType.Smart:
                case SearchType.Exact:
                    return "ms.mol=mol_from_smiles(@SearchText::cstring)";
                case SearchType.Substructure:
                    return "ms.mol@>mol_from_smiles(@SearchText::cstring)";// ORDER BY tanimoto_sml(morganbv_fp(mol_from_smiles(@SearchText::cstring)), ms.fp) DESC";
                case SearchType.Similar:
#warning subject to query corrections
                    return "morganbv_fp(mol_from_smiles(@SearchText::cstring))%ms.fp";// ORDER BY morganbv_fp(mol_from_smiles(@SearchText::cstring))<%>ms.fp";
                case SearchType.Superstructure:
                    return "ms.mol<@mol_from_smiles(@SearchText::cstring)";// ORDER BY tanimoto_sml(morganbv_fp(mol_from_smiles(@SearchText::cstring)), ms.fp) DESC";
                default:
                    throw new ArgumentException();
            }
        }

#warning add query preprocessing to avoid 'bad' queries and possibly optimize the query
        public async Task<IEnumerable<MoleculeData>> FindAsync(SearchQuery query, FilterQuery filters, int skip, int take)
        {
            using (var con = new NpgsqlConnection(_connectionString))
            {
                var command = con.CreateCommand();
                var condition = BuildCondition(query);
                if (filters != null)
                {
                    var filtersClause = BuildFilters(filters);
                    if (filtersClause != "")
                    {
                        condition = $"{condition} AND {filtersClause}";
                    }
                }

                command.CommandText = string.Format(selectFromClause, condition);
                command.Parameters.Add(new NpgsqlParameter("@Limit", take));
                command.Parameters.Add(new NpgsqlParameter("@Offset", skip));
                command.Parameters.Add(new NpgsqlParameter("@SearchText", query.SearchText));

                con.Open();
                var reader = await command.ExecuteReaderAsync();
                var results = new List<MoleculeData>();

                while (reader.Read())
                {
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

        static IEnumerable<string> BuildFilter<T>(FilterQuery.Filter<T>? filter, string colName) where T : struct
        {
            if (filter.HasValue)
            {
                var val = filter.Value;
                if (val.Min.HasValue)
                {
                    yield return $"{colName}>={val.Min.Value}";
                }
                if (val.Max.HasValue)
                {
                    yield return $"{colName}<={val.Max.Value}";
                }
            }
        }

#warning should be reflection
        static string BuildFilters(FilterQuery filters)
        {
            var conditions = BuildFilter(filters.Mw, nameof(molecules_raw.mw))
                .Union(BuildFilter(filters.Logp, nameof(molecules_raw.logp)))
                .Union(BuildFilter(filters.Hba, nameof(molecules_raw.hba)))
                .Union(BuildFilter(filters.Hbd, nameof(molecules_raw.hbd)))
                .Union(BuildFilter(filters.Rotb, nameof(molecules_raw.rotb)))
                .Union(BuildFilter(filters.Tpsa, nameof(molecules_raw.tpsa)))
                .Union(BuildFilter(filters.Fsp3, nameof(molecules_raw.fsp3)))
                .Union(BuildFilter(filters.Hac, nameof(molecules_raw.hac)));

            return string.Join(" AND ", conditions);
        }

        readonly static string oneCommand = $"SELECT " +
            $"{nameof(molecules_raw.smiles)}, " +
            $"{nameof(molecules_raw.name)}, " +
            $"{nameof(molecules_raw.mw)}, " +
            $"{nameof(molecules_raw.logp)}, " +
            $"{nameof(molecules_raw.hba)}, " +
            $"{nameof(molecules_raw.hbd)}, " +
            $"{nameof(molecules_raw.rotb)}, " +
            $"{nameof(molecules_raw.tpsa)}, " +
            $"{nameof(molecules_raw.fsp3)}, " +
            $"{nameof(molecules_raw.hac)} " +
            $"FROM {nameof(molecules_raw)} " +
            $"WHERE idnumber = @{nameof(molecules_raw.idnumber)}";

        public async Task<MoleculeData> ItemAsync(string id)
        {
            using (var con = new NpgsqlConnection(_connectionString))
            {
                var command = con.CreateCommand();
                command.CommandText = oneCommand;

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
