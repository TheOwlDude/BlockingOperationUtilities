using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockingOperationUtilities.DedupingQueue
{
    public class DedupingQueueItemWrapper<T>
    {
        public T Item { get; set; }
        public DedupingQueueItemWrapper(T item)
        {
            Item = item;
        }
    }
}
