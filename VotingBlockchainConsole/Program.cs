using System.Text;
using VotingBlockchain;
using static System.Collections.Specialized.BitVector32;

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

            switch (Console.ReadLine())
            {
                case "1":
                    var elections = await fullNode.Mempool.GetElectionsAsync();
                    if (elections is null) 
                    { 
                        Console.WriteLine("Elections not found");
                        continue;
                    } 
                    foreach (var election in elections)
                        Console.WriteLine("ID: " + election.Index + " Name: " + election.Name + " Start day: " + election.StartDate + " End day: " + election.EndDate);
                    Console.Write("\nChoose one: ");
                    try {
                        var indexOfElection = int.Parse(Console.ReadLine() ?? "") - 1;

                        if (indexOfElection < 0 || indexOfElection >= elections.Count) throw new Exception();

                        Console.WriteLine("Name: " + elections[indexOfElection].Name + " Start day: " + elections[indexOfElection].StartDate + " End day: " + elections[indexOfElection].EndDate + "\n");
                        Console.WriteLine("Uploadin variants for current election...");
                        var variants = await fullNode.Mempool.GetOptionsAsync(elections[indexOfElection].Index);
                        if (variants is null)
                        {
                            Console.WriteLine("Options not found");
                            continue;
                        }
                        for (var i = 0; i < variants.Count; i++)
                            Console.WriteLine((i + 1) + ". " + variants[i].OptionText);
                    }
                    catch { Console.WriteLine("Election not found"); }
                    break;
                case "2":
                    //2
                    break;
                case "3":
                    //3
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