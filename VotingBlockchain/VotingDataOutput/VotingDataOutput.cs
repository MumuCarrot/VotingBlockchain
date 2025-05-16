using VotingBlockchain.Datatypes.Interfaces;

namespace VotingBlockchain.VotingDataOutput
{
    public class VotingDataOutput: IOutput
    {
        public Action<string> _action;

        public VotingDataOutput(Action<string> action) 
        {
            _action = action;
        }

        public void Write(string message) 
        { 
            _action.Invoke(message);
        }
    }
}
