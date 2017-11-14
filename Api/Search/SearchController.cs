using Microsoft.AspNetCore.Mvc;
using Npgsql;
using NpgsqlTypes;
using Search.PostgresRDKit.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Search
{
    [Controller]
    [Route("molecules")]
    public class MoleculesController
    {
        //const string connectionString = "User ID=postgres;Host=rdkit-postgres;Port=5432;Database=simsearch;Pooling=true;";
        const string connectionString = "User ID=postgres;Host=192.168.99.100;Port=5432;Database=simsearch;Pooling=true;";

        [HttpPost]
        [Route("search")]
        public SearchResult Search(string smiles, string searchType, int hitLimit, int pageSize)
        {
            throw new NotImplementedException();
            //try
            //{
            //    using (var con = new NpgsqlConnection(connectionString))
            //    {
            //        var command = con.CreateCommand();
            //        command.CommandText = "SELECT datname FROM pg_database WHERE datistemplate = false;";
            //        //    command.CommandText = "select id, mol from mols where tanimoto_sml(rdkit_fp('CN(CC1=CSC=N1)S(C)(=O)=O'::mol), fp) > 0.7;";
            //        con.Open();
            //        var reader = command.ExecuteReader();
            //        return reader.GetTextReader(0).ReadToEnd();

            //        //    while (reader.Read())
            //        //    {
            //        //        var id = reader["id"];
            //        //        var smiles = reader["mol"];
            //        //    }
            //    }
            //}
            //catch (Exception e)
            //{
            //    return e.ToString();
            //}
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
            $"FROM {nameof(molecules_raw)} WHERE idnumber = @{nameof(molecules_raw.idnumber)}";

        [HttpGet]
        [Route("results/{resultId}")]
        public IEnumerable<MoleculeData> Results([FromRoute]Guid resultId, int pageSize, int pageNumber)
        {
            throw new NotImplementedException();
        }

        [HttpGet]
        [Route("{id}")]
        public MoleculeData One([FromRoute]string id)
        {
            using (var con = new NpgsqlConnection(connectionString))
            {
                var command = con.CreateCommand();
                command.CommandText = oneCommand;
                command.Parameters.Add(new NpgsqlParameter($"@{nameof(molecules_raw.idnumber)}", id));

                con.Open();
                var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    return new MoleculeData
                    {
                        Idnumber = id,
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
