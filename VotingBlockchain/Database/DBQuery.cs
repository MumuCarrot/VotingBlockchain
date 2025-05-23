﻿using System.Text.RegularExpressions;
using VotingBlockchain.Datatypes.Classes;

namespace VotingBlockchain
{
    public partial class FullNode
    {
        public partial class Blockchain
        {
            public static partial class DBQuery
            {
                private static readonly DBAdapter _adapter;
                static DBQuery()
                {
                    _adapter = new DBAdapter();
                }

                public static async Task<List<Election>?> GetElectionsAsync()
                {
                    string query = "SELECT * FROM elections";
                    var dict = await _adapter.ExecuteQueryAsync(query);

                    if (dict is null || dict.Count == 0) return null;

                    var elections = new List<Election>();
                    foreach (var i in dict)
                    {
                        elections.Add(new Election()
                        {
                            Id = (int)i["id"],
                            Name = (string)i["name"],
                            StartDate = (long)i["startdate"],
                            EndDate = (long)i["enddate"],
                            Description = (string)i["description"],
                        });
                    }
                    return elections;
                }

                public static async Task<Election?> GetElectionAsync(int electionId)
                {
                    string query = "SELECT * FROM elections WHERE id = @electionId";
                    var parameters = new Dictionary<string, object>()
                    {
                        { "electionId", electionId }
                    };
                    var dict = await _adapter.ExecuteQueryAsync(query, parameters);

                    if (dict is null || dict.Count == 0) return null;

                    return new Election()
                    {
                        Id = (int)dict[0]["id"],
                        Name = (string)dict[0]["name"],
                        StartDate = (long)dict[0]["startdate"],
                        EndDate = (long)dict[0]["enddate"],
                        Description = (string)dict[0]["description"]
                    };
                }

                public static async Task<List<Election>?> GetCurrentElectionsAsync(long unix)
                {
                    string query = "SELECT * FROM elections where startdate <= @unix AND @unix < enddate;";
                    var parameters = new Dictionary<string, object>()
                    {
                        { "unix", unix }
                    };
                    var dict = await _adapter.ExecuteQueryAsync(query, parameters);
                    if (dict is null || dict.Count == 0) return null;

                    var elections = new List<Election>();
                    foreach (var i in dict)
                    {
                        elections.Add(new Election()
                        {
                            Id = (int)i["id"],
                            Name = (string)i["name"],
                            StartDate = (long)i["startdate"],
                            EndDate = (long)i["enddate"],
                            Description = (string)i["description"],
                        });
                    }
                    return elections;
                }

                public static async Task<List<Election>?> GetCompletedElectionsAsync(long unix)
                {
                    string query = "SELECT * FROM elections WHERE enddate <= @unix";
                    var parameters = new Dictionary<string, object>()
                    {
                        { "unix", unix }
                    };
                    var dict = await _adapter.ExecuteQueryAsync(query, parameters);

                    if (dict is null || dict.Count == 0) return null;

                    var elections = new List<Election>();
                    foreach (var i in dict)
                    {
                        elections.Add(new Election()
                        {
                            Id = (int)i["id"],
                            Name = (string)i["name"],
                            StartDate = (long)i["startdate"],
                            EndDate = (long)i["enddate"],
                            Description = (string)i["description"],
                        });
                    }
                    return elections;
                }

                public static async Task<List<Option>?> GetOptionsAsync(int id)
                {
                    string query = "SELECT * FROM options WHERE electionid = @id";
                    var parameters = new Dictionary<string, object>()
                    {
                        { "id", id }
                    };
                    var dict = await _adapter.ExecuteQueryAsync(query, parameters);
                    if (dict is null || dict.Count == 0) return null;
                    var votes = new List<Option>();
                    foreach (var i in dict)
                    {
                        votes.Add(new Option()
                        {
                            Index = (int)i["id"],
                            ElectionId = (int)i["electionid"],
                            OptionText = (string)i["optiontext"]
                        });
                    }
                    votes.Sort((o1, o2) => o1.Index.CompareTo(o2.Index));
                    return votes;
                }

                public static async Task<List<string>?> RegisterAsync(string username, string password, Regex? regex = null)
                {
                    if (regex is null)
                        regex = new Regex(@"^(?:[А-ЩЬЮЯҐЄІЇ]{2}\d{6}|\d{9})$");

                    if (!regex.IsMatch(username))
                        return null;

                    var temp = User.NewUser(username, password);
                    string query = "INSERT INTO users (username, password, publicKey) VALUES (@username, @password, @publicKey)";
                    var parameters = new Dictionary<string, object>
                    {
                        { "username", temp.Username },
                        { "password", temp.PasswordHash },
                        { "publicKey", temp.PublicKey }
                    };
                    await _adapter.ExecuteNonQueryAsync(query, parameters);

                    return [temp.PublicKey, temp.PrivateKey];
                }

                public static async Task<User?> AuthenticateAsync(string username, string password)
                {
                    string query = "SELECT * FROM users WHERE username = @username AND password = @password";
                    User temp = new User(username, password);
                    var parameters = new Dictionary<string, object>
                    {
                        { "username", temp.Username },
                        { "password", temp.PasswordHash }
                    };

                    var users = await _adapter.ExecuteQueryAsync(query, parameters);
                    if (!(users.Count > 0)) return null;

                    temp.Id = users[0]["id"].ToString()!;
                    temp.Username = users[0]["username"].ToString()!;
                    temp.PasswordHash = users[0]["password"].ToString()!;
                    temp.PublicKey = users[0]["publickey"].ToString()!;
                    temp.Role = int.Parse(users[0]["role"].ToString()!);

                    return temp;
                }

                public static async Task<bool> UserExistAsync(string username)
                {
                    string query = "SELECT COUNT(*) FROM users WHERE username = @username";
                    var parameters = new Dictionary<string, object>
                    {
                        { "username", username }
                    };
                    var result = await _adapter.ExecuteScalarAsync(query, parameters);

                    return Convert.ToInt32(result) > 0;
                }

                public static async Task<Block?> GetLatestBlockAsync(int electionId)
                {
                    string query = "SELECT * FROM block_chain_votes WHERE election_id = @election_id ORDER BY id DESC LIMIT 1;";
                    var parameters = new Dictionary<string, object>()
                    {
                        { "election_id", electionId }
                    };

                    var dict = await _adapter.ExecuteQueryAsync(query, parameters);
                    if (dict is null || dict.Count == 0) return null;

                    return new Block(
                        (int)dict[0]["index"],
                        (int)dict[0]["election_id"],
                        (long)dict[0]["timestamp"],
                        (string)dict[0]["previous_hash"],
                        (string)dict[0]["this_hash"],
                        (int)dict[0]["nonce"],
                        (int)dict[0]["difficulty"],
                        (string)dict[0]["encrypted_data"],
                        (string)dict[0]["public_data"]
                    );
                }

                public static async Task<List<Block>?> GetBlockChainAsync(int electionId, bool isDesc = false)
                {
                    string query = "";
                    if (!isDesc)
                        query = "SELECT * FROM block_chain_votes WHERE election_id = @election_id;";
                    else
                        query = "SELECT * FROM block_chain_votes WHERE election_id = @election_id ORDER BY id DESC;";
                    var parameters = new Dictionary<string, object>()
                    {
                        { "election_id", electionId }
                    };

                    var dict = await _adapter.ExecuteQueryAsync(query, parameters);
                    if (dict is null || dict.Count == 0) return null;

                    List<Block> temp = [];
                    foreach (var d in dict)
                    {
                        var b = new Block(
                            (int)d["index"],
                            (int)d["election_id"],
                            (long)d["timestamp"],
                            (string)d["previous_hash"],
                            (string)d["this_hash"],
                            (int)d["nonce"],
                            (int)d["difficulty"],
                            (string)d["encrypted_data"],
                            (string)d["public_data"]
                        );
                        temp.Add(b);
                    }
                    return temp;
                }

                public static async Task AddBlockAsync(Block block)
                {
                    string query = "INSERT INTO block_chain_votes (election_id, index, timestamp, previous_hash, this_hash, nonce, difficulty, encrypted_data, public_data) " +
                                   "VALUES (@election_id, @index, @timestamp, @previous_hash, @this_hash, @nonce, @difficulty, @encrypted_data, @public_data);";
                    var parameters = new Dictionary<string, object>()
                    {
                        { "election_id", block.ElectionId },
                        { "index", block.Index },
                        { "timestamp", block.Timestamp },
                        { "previous_hash", block.PreviousHash },
                        { "this_hash", block.ThisHash },
                        { "nonce", block.Nonce },
                        { "difficulty", block.Difficulty },
                        { "encrypted_data", block.EncryptedData },
                        { "public_data", block.PublicData }
                    };
                    await _adapter.ExecuteNonQueryAsync(query, parameters);
                }

                public static async Task<int?> GetNCounterAsync(int userId, int electionId)
                {
                    string query = "SELECT voters.n_counter FROM voters WHERE voter_id = @voterid AND election_id = @electionid;";
                    var parameters = new Dictionary<string, object>
                    {
                        { "electionid", electionId },
                        { "voterid", userId }
                    };
                    var result = await _adapter.ExecuteScalarAsync(query, parameters);

                    var i = (int?)result;

                    return i;
                }

                public static async Task IncreaseNCounterAsync(int userId, int electionId)
                {
                    string query = "MERGE INTO voters AS v " +
                                   "USING (SELECT @voterid AS voter_id, @electionid AS election_id) AS new_data " +
                                   "ON v.election_id = new_data.election_id AND v.voter_id = new_data.voter_id " +
                                   "WHEN MATCHED THEN " +
                                       "UPDATE SET n_counter = v.n_counter + 1 " +
                                   "WHEN NOT MATCHED THEN " +
                                       "INSERT (voter_id, election_id, n_counter) " +
                                       "VALUES (new_data.voter_id, new_data.election_id, 1);";
                    var parameters = new Dictionary<string, object>
                    {
                        { "voterid", userId },
                        { "electionid", electionId }
                    };
                    await _adapter.ExecuteNonQueryAsync(query, parameters);
                }

                public static async Task<int?> GetRevotesAsync(int electionId)
                {
                    string query = "SELECT elections.revote FROM elections WHERE id = @electionid;";
                    var parameters = new Dictionary<string, object>
                    {
                        { "electionid", electionId }
                    };
                    var result = await _adapter.ExecuteScalarAsync(query, parameters);

                    var i = (int?)result;

                    return i;
                }

                public static async Task<List<Election>?> GetVotedElectionsAsync(int userId)
                {
                    string query = "SELECT e.* FROM elections e JOIN voters v ON e.id = v.election_id WHERE v.voter_id = @voterid;";
                    var parameters = new Dictionary<string, object>
                    {
                        { "voterid", userId }
                    };
                    var result = await _adapter.ExecuteQueryAsync(query, parameters);

                    var elections = new List<Election>();
                    foreach (var i in result)
                    {
                        elections.Add(new Election()
                        {
                            Id = (int)i["id"],
                            Name = (string)i["name"],
                            StartDate = (long)i["startdate"],
                            EndDate = (long)i["enddate"],
                            Description = (string)i["description"],
                        });
                    }
                    return elections;
                }

                public static async Task PostElection(string @electionname, long @startdate, long @enddate, string @description, int @revote, string options) 
                {
                    string query = "WITH inserted_election AS ( " +
                                       "INSERT INTO elections (name, startDate, endDate, description, revote) " +
                                       "VALUES (@electionname, @startdate, @enddate, @description, @revote) " +
                                       "RETURNING id " +
                                       "), " +
                                   "inserte_options AS ( " +
                                       "INSERT INTO options (electionId, optionText) " +
                                       "SELECT id, optionText " +
                                       "FROM inserted_election, " +
                                           "(VALUES";

                    var parameters = new Dictionary<string, object>
                    {
                        { "@electionname", @electionname },
                        { "@startdate", @startdate },
                        { "@enddate", @enddate },
                        { "@description", @description },
                        { "@revote", @revote }
                    };

                    options = options.Replace("\r", "");
                    string[] optionList = options.Split(['\n']);

                    string queryOptions = "";
                    for (var i = 0; i < optionList.Length; i++) 
                    {
                        queryOptions += $" (@option{i}),";
                        parameters.Add($"@option{i}", optionList[i]);
                    }
                    queryOptions = queryOptions.TrimEnd(',');
                    queryOptions += ") AS vals(optionText)) SELECT id FROM inserted_election;";
                    query += queryOptions;

                    var result = await _adapter.ExecuteScalarAsync(query, parameters);
                    if (result is null)
                        throw new Exception("Failed to insert election and options.");
                    Block gen = Block.CreateGenesisBlock((int)result);
                    await AddBlockAsync(gen);
                }
            }
        }
    }
}