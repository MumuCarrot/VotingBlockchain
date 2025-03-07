using VotingBlockchain.Interfaces;

namespace VotingBlockchain
{
    public class Output: IOutput
    {
        public Action<string> _action;

        public Output(Action<string> action) 
        {
            _action = action;
        }

        public void Write(string message) 
        { 
            _action.Invoke(message);
        }
    }
}
