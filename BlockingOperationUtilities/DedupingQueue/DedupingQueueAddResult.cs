using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockingOperationUtilities.DedupingQueue
{
    public enum DedupingQueueAddResult
    {
        Unknown = 0,
        Duplicate = 1,
        NewItem = 2,
        QueueFull = 3
    }
}
