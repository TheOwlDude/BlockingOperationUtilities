using BlockingOperationUtilities.DedupingQueue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BlockingOperationUtilities
{
    /// <summary>
    /// This class allows producers to submit items to be processed async in a 'Fire and Forget' manner. 
    /// 
    /// The original use case motivating this class was publishing to ActiveMQ using a failover connection. The failover connection provides many benefits
    /// but a side effect of using it is that if the underlying connection is broken, every call blocks until the connection is reestablished. If not 
    /// addressed the blocking behavior causes all sorts of problems in the client application, among them the service might not shut down.
    /// 
    /// The best way to address the blocking behavior depends on the nature of the messages being published. In the original application motivating this 
    /// library there were a number of heartbeat and status update messages. These kinds of messages are sent frequently and only the most recent is 
    /// interesting. If a message of this sort is not sent in a timely manner it is not worth sending at all. The TransientItemWorkUtility class is intended 
    /// for these types of messages, they may not get processed, and their producers don't especially care, a new one will come down the pipe shortly.
    /// 
    /// This motivates the use of the DedupingQueue. A producer of a transient message can assign their messages an id. When supplying an id only the most
    /// recent message for that id is kept in the queue. In the case where there is an interval of blocking, keeping only the most recent message for each
    /// id restricts memory usage by the queue. It also spares the consumer of theses messages receiving a flood of them once the blockage is cleared when
    /// all the consumer cares about is the most recent.
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
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
            //The thread could be started in the constructor. My experience is that as these classes evolve switches and conditional behaviors are added that need
            //to be initialized before the thread is started. If the thead is started in the constructor all of the new switches/properties need to be added as 
            //constructor parameters. If the thread is started in a separate method like here, the additional behaviors can be controlled by properties set prior
            //to calling Start() and this is less disruptive to existing clients who don't care about the new behaviors.

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
