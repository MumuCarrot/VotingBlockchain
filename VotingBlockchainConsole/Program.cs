using System.Text;
using VotingBlockchain;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

FullNode fullNode = new FullNode();
Miner miner = new Miner(fullNode);

fullNode.HostServer();
miner.Mine();

while (true)
{
    string username = "";
    string userPassword = "";

    Console.Write("Enter user username: ");
    username = Console.ReadLine() ?? "";

    if (!(await fullNode.UserDatabase.UserExistAsync(username)))
    {
        Console.WriteLine("User doesn`t exists");
        Console.Write("Enter user password: ");
        userPassword = Console.ReadLine() ?? "";
        var keys = await fullNode.UserDatabase.RegisterAsync(username, userPassword) ?? throw new Exception("Keys is null");

        if (keys.Count != 2) throw new Exception("Keys count != 2");

        Console.WriteLine("\t!!!!!!!!!!WARNING!!!!!!!!!!");
        Console.WriteLine("\t!!!SAVE YOUR PRIVATE KEY!!!");
        Console.WriteLine("\t!!!!!!!!!!WARNING!!!!!!!!!!");
        Console.WriteLine();
        Console.WriteLine("Private key: " + keys[1]);
    }
    else
    {
        Console.WriteLine("User exists");
        Console.Write("Enter user password: ");
        userPassword = Console.ReadLine() ?? "";
    }

    User? user = await fullNode.UserDatabase.AuthenticateAsync(username, userPassword);

    if (user != null)
    {
        Console.WriteLine("Authorisation passed!");
        Console.WriteLine($"Welcome back {(user.Role == 0 ? "user" : "admin")} {user.Username}");

        bool logedIn = true;
        while (logedIn)
        {

            Console.WriteLine("Action list:\n1. Vote\n2. My votes\n3. Results\n4. Log out");
            var action = Console.ReadLine() ?? "";

            var elections = await fullNode.Mempool.GetElectionsAsync();
            if (elections is null)
            {
                Console.WriteLine("Elections not found");
                continue;
            }

            switch (action)
            {
                case "1":
                    foreach (var election in elections)
                        Console.WriteLine("ID: " + election.Id + " Name: " + election.Name + " Start day: " + election.StartDate + " End day: " + election.EndDate);
                    Console.Write("\nChoose one: ");
                    try
                    {
                        var indexOfElection = int.Parse(Console.ReadLine() ?? "") - 1;

                        if (indexOfElection < 0 || indexOfElection >= elections.Count) throw new ArgumentException();

                        Console.WriteLine("Name: " + elections[indexOfElection].Name + " Start day: " + elections[indexOfElection].StartDate + " End day: " + elections[indexOfElection].EndDate + "\n");
                        Console.WriteLine("Uploadin variants for current election...");
                        var options = await fullNode.Mempool.GetOptionsAsync(elections[indexOfElection].Id);
                        if (options is null)
                        {
                            Console.WriteLine("Options not found");
                            continue;
                        }
                        for (var i = 0; i < options.Count; i++)
                            Console.WriteLine((i + 1) + ". " + options[i].OptionText);

                        Console.Write("\nChoose one: ");
                        var indexOfOption = int.Parse(Console.ReadLine() ?? "") - 1;

                        if (indexOfOption < 0 || indexOfOption >= options.Count) throw new ArgumentException();

                        await fullNode.Blockchain.VoteAsync(elections[indexOfElection].Id, user, options[indexOfOption].Index);
                    }
                    catch (ArgumentException) 
                    { 
                        Console.WriteLine("No option like this"); 
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error: " + ex.Message);
                    }
                    break;
                case "2":
                    //2
                    break;
                case "3":
                    foreach (var election in elections)
                        Console.WriteLine("ID: " + election.Id + " Name: " + election.Name + " Start day: " + election.StartDate + " End day: " + election.EndDate);
                    Console.Write("\nChoose one: ");
                    try
                    {
                        var indexOfElection = int.Parse(Console.ReadLine() ?? "") - 1;

                        if (indexOfElection < 0 || indexOfElection >= elections.Count) throw new ArgumentException();

                        await fullNode.Blockchain.GetElectionResults(elections[indexOfElection].Id);
                    }
                    catch (ArgumentException)
                    {
                        Console.WriteLine("No option like this");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error: " + ex.Message);
                    }
                    break;
                case "4":
                    logedIn = false;
                    break;
                default:
                    Console.WriteLine("No such option");
                    break;
            }
        }
    }
    else
    {
        Console.WriteLine("Ошибка авторизации!");
        Console.WriteLine("Пасспорт пользователя содержит не допустимый паттерн, попробуйте заново");
    }
}