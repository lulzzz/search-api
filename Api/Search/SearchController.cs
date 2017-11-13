using Microsoft.AspNetCore.Mvc;
using Npgsql;
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
        [HttpPost]
        [Route("search")]
        public string Search(
            //[FromBody]SearchRequest request
            )
        {
            try
            {
                using (var con = new NpgsqlConnection("User ID=postgres;Host=rdkit-postgres;Port=5432;Database=postgres;Pooling=true;"))
                {
                    var command = con.CreateCommand();
                    command.CommandText = "SELECT datname FROM pg_database WHERE datistemplate = false;";
                    //    command.CommandText = "select id, mol from mols where tanimoto_sml(rdkit_fp('CN(CC1=CSC=N1)S(C)(=O)=O'::mol), fp) > 0.7;";
                    con.Open();
                    var reader = command.ExecuteReader();
                    return reader.GetTextReader(0).ReadToEnd();

                    //    while (reader.Read())
                    //    {
                    //        var id = reader["id"];
                    //        var smiles = reader["mol"];
                    //    }
                }
            }
            catch (Exception e)
            {
                return e.ToString();
            }
            

            //throw new NotImplementedException();
        }

        [HttpGet]
        [Route("{id}")]
        public IEnumerable<MoleculeData> One([FromRoute]string id)
        {
            throw new NotImplementedException();
        }
    }
}
