using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Search
{
    [Controller]
    public class SearchController
    {
        [HttpPost]
        [Route("search")]
        public IEnumerable<SearchHit> Search(
            //[FromBody]SearchRequest request
            )
        {
            using (var con = new NpgsqlConnection("User ID=postgres;Password=123456;Host=192.168.99.101;Port=5432;Database=simsearch;Pooling=true;"))
            {
                var command = con.CreateCommand();
                command.CommandText = "select id, mol from mols where tanimoto_sml(rdkit_fp('CN(CC1=CSC=N1)S(C)(=O)=O'::mol), fp) > 0.7;";
                con.Open();
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var id = reader["id"];
                    var smiles = reader["mol"];
                }
            }

            return null;
        }

        public class SearchHit
        {
        }
    }
}
