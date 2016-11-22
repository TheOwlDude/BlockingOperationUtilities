using BlockingOperationUtilities.DedupingQueue;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockingOperationUtilityTests
{
    public class DedupingQueueTests
    {
        [Test]
    public void DequeueWhenEmptyReturnsNull()
        {
            DedupingQueue<DedupableInt> queue = new DedupingQueue<DedupableInt>();
            Assert.IsNull(queue.Dequeue());
        }

        [Test]
        public void ItemWithNullTokenIsEnqueuedAtEnd()
        {
            DedupingQueue<DedupableInt> queue = new DedupingQueue<DedupableInt>();

            queue.AddItem(new DedupableInt(3, "foo"));
            queue.AddItem(new DedupableInt(4, null));

            Assert.Equals(2, queue.Size);
            Assert.Equals(3, queue.Dequeue().getVal());
            Assert.Equals(4, queue.Dequeue().getVal());
        }

        [Test]
        public void ItemWithMatchingTokenReplaces()
        {
            DedupingQueue<DedupableInt> queue = new DedupingQueue<DedupableInt>();

            queue.AddItem(new DedupableInt(3, "foo"));
            queue.AddItem(new DedupableInt(4, null));
            queue.AddItem(new DedupableInt(5, "foo"));

            Assert.Equals(2, queue.Size);
            Assert.Equals(5, queue.Dequeue().getVal());
            Assert.Equals(4, queue.Dequeue().getVal());
        }

        [Test]
        public void IfNotAcceptedNotInHashtable()
        {
            DedupingQueue<DedupableInt> queue = new DedupingQueue<DedupableInt>(2);

            queue.AddItem(new DedupableInt(3, "foo"));
            queue.AddItem(new DedupableInt(4, null));

            //This item will not be placed into the queue because the queue is full. This means that it should not be placed
            //in the hashtable either becuase then after a matched id replace the object would STILL not be in the queue
            queue.AddItem(new DedupableInt(5, "bar"));

            Assert.Equals(2, queue.size());

            Assert.Equals(3, queue.Dequeue().getVal());
            queue.AddItem(new DedupableInt(6, "bar"));
            Assert.Equals(4, queue.Dequeue().getVal());
            Assert.Equals(6, queue.Dequeue().getVal());
        }

        [Test]
        public void AfterDequeueNotInHashtable()
        {
            DedupingQueue<DedupableInt> queue = new DedupingQueue<DedupableInt>();

            queue.AddItem(new DedupableInt(3, "foo"));
            Assert.Equals(3, queue.Dequeue().getVal());

            //After the dequeue above the item is gone from the queue. If the item were still in the hashtable then
            //queueing the same token would cause a replace for an item not in the queue
            queue.AddItem(new DedupableInt(4, "foo"));
            Assert.Equals(4, queue.Dequeue().getVal());
        }

    }
}
