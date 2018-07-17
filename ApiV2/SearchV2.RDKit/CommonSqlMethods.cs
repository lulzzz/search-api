using Npgsql;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SearchV2.RDKit
{
    static class CommonSqlMethods
    {
        public static async Task WithConnection(string connectionString, Func<NpgsqlConnection, Task> asyncAction)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                await connection.OpenAsync();
                await asyncAction(connection);
            }
        }

        public static async Task RunTransactionAndCommit(this NpgsqlConnection c, Func<NpgsqlTransaction, Task> asyncAction)
        {
            using (var t = c.BeginTransaction())
            {
                await asyncAction(t);
                await t.CommitAsync();
            }
        }

        public static async Task ExecuteCommandNonQuery(this NpgsqlConnection c, string command)
        {
            using (var cmd = new NpgsqlCommand(command, c))
            {
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public static async Task ExecuteCommandNonQuery(this NpgsqlTransaction t, string command, params NpgsqlParameter[] parameters)
        {
            using (var cmd = new NpgsqlCommand(command, t.Connection))
            {
                if (parameters != null) {
                    cmd.Parameters.AddRange(parameters);
                }
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public static async Task<object> ExecuteCommandScalar(this NpgsqlConnection c, string command)
        {
            using (var cmd = new NpgsqlCommand(command, c))
            {
                return await cmd.ExecuteScalarAsync();
            }
        }
    }
}
