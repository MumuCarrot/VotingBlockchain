namespace VotingBlockchain.Interfaces
{
    public interface INode
    {
        //Обеспечение доступа к данным
        public Mempool Mempool { get; }

        //Хранение данных
        //Валидация транзакций (голосов)
        //Формирование новых блоков
        //Проверка целостности блокчейна
        public Blockchain Blockchain { get; }

        //Распределение информации по сети
        public void HostServer();
    }
}
