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
        private Thread publishThread;

        Action<T> blockingOperation;

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
