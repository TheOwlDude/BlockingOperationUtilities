using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockingOperationUtilities.DedupingQueue
{
    public class DedupingQueue<T>
    {
        private Dictionary<String, DedupingQueueItemWrapper<T>> itemsWithTokens = new Dictionary<string, DedupingQueueItemWrapper<T>>();
        private Queue<DedupingQueueItemWrapper<T>> queue;
        private object itemSync = new object();
        private int? maxCapacity = null;

        public DedupingQueue():this(null)
        { }

        public DedupingQueue(int? maxCapacity)
        {
            this.maxCapacity = maxCapacity;
            queue = this.maxCapacity != null ? 
                new Queue<DedupingQueueItemWrapper<T>>(maxCapacity.Value) : 
                new Queue<DedupingQueueItemWrapper<T>>();
        }

        public DedupingQueueAddResult AddItem(T item)
        {
            lock (itemSync)
            {
                string tokenValue = GetToken(item);
                DedupingQueueItemWrapper<T> wrappedItem;
                if (tokenValue != null && itemsWithTokens.TryGetValue(tokenValue, out wrappedItem)) //if token is already in queue then just swap.
                {
                    wrappedItem.Item = item;
                    return DedupingQueueAddResult.Duplicate;
                }

                //if no more room than item is dropped
                if (maxCapacity != null && queue.Count >= maxCapacity.Value) return DedupingQueueAddResult.QueueFull;

                DedupingQueueItemWrapper<T> transientItemWrapper = new DedupingQueueItemWrapper<T>(item);
                queue.Enqueue(transientItemWrapper);
                if (tokenValue != null) itemsWithTokens[tokenValue] = transientItemWrapper;
                return DedupingQueueAddResult.NewItem;
            }
        }

        public T Dequeue()
        {
            T result = default(T);
            lock (itemSync)
            {
                if (queue.Count > 0)
                {
                    DedupingQueueItemWrapper<T> dequeuedItemWrapper = queue.Dequeue();
                    result = dequeuedItemWrapper.Item;
                    String idToken = GetToken(result);
                    if (idToken != null) itemsWithTokens.Remove(idToken);
                }
            }
            return result;
        }

        public int Size {  get { return queue.Count; } }

        private string GetToken(T item)
        {
            IDedupable itemAsDedupable = item as IDedupable;
            return itemAsDedupable != null ? itemAsDedupable.GetIdentifyingToken() : null;
        }

    }
}
