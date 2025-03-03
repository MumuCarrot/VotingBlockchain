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
    Console.Write("Введите паспорт пользователя: ");
    string userId = Console.ReadLine() ?? "";
    
    fullNode.UserDatabase.Register(userId, "securePassword");
    User? user = fullNode.UserDatabase.Authenticate(userId, "securePassword");

    if (user != null)
    {
        Console.WriteLine("Авторизация успешна!");

        Console.WriteLine("Чо хатите делать?\n1. ГОЛОСОВАТЬ ХОЧУ\n2. Пасатреть свой голос\n3. Давай результаты блина");

        switch (Console.ReadLine()) 
        {
            case "1":
                while (true)
                {
                    Console.WriteLine("Выборы выборвы кондидаты:\n1. A\n2. B\n0. Exit");

                    var key = Console.ReadKey();
                    if (key.KeyChar == '1')
                        fullNode.Blockchain.VoteOrUpdate(user, "Candidate A");
                    else if (key.KeyChar == '2')
                        fullNode.Blockchain.VoteOrUpdate(user, "Candidate B");
                    else if (key.KeyChar == '0')
                    {
                        fullNode.Blockchain.EndVoting(user);
                        break;
                    }
                    else
                        Console.WriteLine("Такова кандидата нет, увы, хех.");
                }
                break;
            case "2":
                string userVote = fullNode.Blockchain.GetUserVote(user) ?? "Голос проходит обработку ну или вы еще не голосовали";
                Console.WriteLine($"Ваш голос: {userVote}");
                break;
            case "3":
                var res = fullNode.Blockchain.GetCurrentResults();
                foreach (var result in res.Result) 
                { 
                    Console.WriteLine($"{result.Key}\t{result.Value}");
                }
                break;
            default:
                Console.WriteLine("Нет такого варианта");
                break;
        }
    }
    else
    {
        Console.WriteLine("Ошибка авторизации!");
        Console.WriteLine("Пасспорт пользователя содержит не допустимый паттерн, попробуйте заново");
    }
}



