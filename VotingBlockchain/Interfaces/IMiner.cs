using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VotingBlockchain.Interfaces
{
    public interface IMiner
    {
        public void Mine();

        public void Rest();
    }
}
