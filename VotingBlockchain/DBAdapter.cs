using Npgsql;

namespace VotingBlockchain
{
    public partial class FullNode
    {
        public partial class Blockchain
        {
            public static partial class DBQuery
            {
                public class DBAdapter
                {
                    private const string _connectionString = "Server=localhost;Port=5432;User Id=postgres;Password=8080;Database=postgres;";

                    public async Task ExecuteNonQueryAsync(string query, Dictionary<string, object>? parameters = null)
                    {
                        await using var connection = new NpgsqlConnection(_connectionString);
                        await connection.OpenAsync();
                        await using var cmd = new NpgsqlCommand(query, connection);

                        if (parameters != null)
                        {
                            foreach (var param in parameters)
                            {
                                cmd.Parameters.AddWithValue(param.Key, param.Value);
                            }
                        }

                        await cmd.ExecuteNonQueryAsync();
                    }

                    public async Task<List<Dictionary<string, object>>> ExecuteQueryAsync(string query, Dictionary<string, object>? parameters = null)
                    {
                        var results = new List<Dictionary<string, object>>();

                        await using var connection = new NpgsqlConnection(_connectionString);
                        await connection.OpenAsync();
                        await using var cmd = new NpgsqlCommand(query, connection);

                        if (parameters != null)
                        {
                            foreach (var param in parameters)
                            {
                                cmd.Parameters.AddWithValue(param.Key, param.Value);
                            }
                        }

                        await using var reader = await cmd.ExecuteReaderAsync();
                        while (await reader.ReadAsync())
                        {
                            var row = new Dictionary<string, object>();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                row[reader.GetName(i)] = reader.GetValue(i);
                            }
                            results.Add(row);
                        }
                        return results;
                    }

                    public async Task<object?> ExecuteScalarAsync(string query, Dictionary<string, object>? parameters = null)
                    {
                        await using var connection = new NpgsqlConnection(_connectionString);
                        await connection.OpenAsync();
                        await using var command = new NpgsqlCommand(query, connection);

                        if (parameters != null)
                        {
                            foreach (var param in parameters)
                            {
                                command.Parameters.AddWithValue(param.Key, param.Value);
                            }
                        }

                        return await command.ExecuteScalarAsync();
                    }
                }
            }
        }
    }
}
