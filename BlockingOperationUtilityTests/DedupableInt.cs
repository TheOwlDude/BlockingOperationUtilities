using BlockingOperationUtilities.DedupingQueue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockingOperationUtilityTests
{
    class DedupableInt : IDedupable
    {
        private int val;
        private string token;

        public DedupableInt(int val, String token)
        {
            this.val = val;
            this.token = token;
        }

        public int getVal() { return val; }

        public string GetIdentifyingToken() { return token; }
    }
}
