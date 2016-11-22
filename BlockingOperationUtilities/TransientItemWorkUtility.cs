using BlockingOperationUtilities.DedupingQueue;
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
        private DedupingQueue<T> queue;
        Action<T> blockingOperation;

        public TransientItemWorkUtility(Action<T> blockingOperation, string publishThreadName):this(blockingOperation, publishThreadName, null)
        {}

        public TransientItemWorkUtility(Action<T> blockingOperation, string publishThreadName, int? maxCapacity)
        {
            this.blockingOperation = blockingOperation;
            queue = new DedupingQueue<T>(maxCapacity);

            publishThread = new Thread(new ThreadStart(WorkThreadProc));
            publishThread.Name = publishThreadName;     
            publishThread.IsBackground = true;          //making thread background allows process to terminate when the operation is blocked.
        }

        /// <summary>
        /// Begins processing of work queue
        /// </summary>
        public void Start()
        {
            publishThread.Start();
        }

        public DedupingQueueAddResult Add(T item)
        {
            return queue.AddItem(item);
        }

        private void WorkThreadProc()
        {
            while(true)
            {
                T item;
                if (!queue.Dequeue(out item))
                {
                    Thread.Sleep(50);
                    continue;
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

        protected virtual void HandleOperationErrors(Exception e) {}
    }
}
