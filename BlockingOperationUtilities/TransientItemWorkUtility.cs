using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BlockingOperationUtilities
{
    public class TransientItemWorkUtility<T> 
    {
        private Dictionary<String, TransientItemWrapper<T>> itemsWithTokens = new Dictionary<string, TransientItemWrapper<T>>();
        private Queue<TransientItemWrapper<T>> queue;
        private object itemSync = new object();
        private Thread publishThread;

        Action<T> blockingOperation;
        private int? maxCapacity = null;

        public TransientItemWorkUtility(Action<T> blockingOperation, string publishThreadName):this(blockingOperation, publishThreadName, null)
        {}

        public TransientItemWorkUtility(Action<T> blockingOperation, string publishThreadName, int? maxCapacity)
        {
            this.blockingOperation = blockingOperation;
            this.maxCapacity = maxCapacity;
            queue = maxCapacity == null ? new Queue<TransientItemWrapper<T>>() : new Queue<TransientItemWrapper<T>>(maxCapacity.Value);

            publishThread = new Thread(new ThreadStart(WorkThreadProc));
            publishThread.Name = publishThreadName;
            publishThread.IsBackground = true;
        }

        public void AddWorkItem(T item)
        {
            lock(itemSync)
            {
                string tokenValue = GetToken(item);
                TransientItemWrapper<T> wrappedItem;
                if (tokenValue != null && itemsWithTokens.TryGetValue(tokenValue, out wrappedItem)) //if token is already in queue then just swap.
                {
                    wrappedItem.Item = item;
                    return;
                }

                //if no more room than item is dropped
                if (maxCapacity != null && queue.Count >= maxCapacity.Value) return;

                TransientItemWrapper<T> transientItemWrapper = new TransientItemWrapper<T>(item);
                queue.Enqueue(transientItemWrapper);
                if (tokenValue != null) itemsWithTokens[tokenValue] = transientItemWrapper;
            }
        }

        private void WorkThreadProc()
        {
            bool encounteredEmptyQueue = false;
            while(true)
            {
                if (encounteredEmptyQueue)
                {
                    Thread.Sleep(50);
                    encounteredEmptyQueue = false;
                }

                T item;
                lock(itemSync)
                {
                    if (queue.Count == 0)   //Attempting to Dequeue from an empty queue throws InvalidOperationException
                    {
                        encounteredEmptyQueue = true;     //don't want to sleep inside the lock
                        continue;
                    }

                    TransientItemWrapper<T> wrapper = queue.Dequeue();
                    item = wrapper.Item;
                    string tokenValue = GetToken(item);
                    if (tokenValue != null) itemsWithTokens.Remove(tokenValue);
                }

                try
                {
                    blockingOperation(item);
                }
                catch(Exception e)
                {
                    try { HandleOperationErrors(e); }   //don't let problems in the extension's error handling crash the thread
                    catch (Exception) { }
                }
            }
        }

        private string GetToken(T item)
        {
            IAmIDentifiable itemAsIAmIdentifiable = item as IAmIDentifiable;
            return itemAsIAmIdentifiable != null ? itemAsIAmIdentifiable.IdentifyingToken() : null;
        }

        public virtual void HandleOperationErrors(Exception e)
        {

        }
    }
}
