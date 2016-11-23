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
    public void DequeueWhenEmptyReturnsFalse()
        {
            DedupingQueue<DedupableInt> queue = new DedupingQueue<DedupableInt>();
            DedupableInt item;
            Assert.IsFalse(queue.Dequeue(out item));
            Assert.IsNull(item);
        }

        [Test]
        public void ItemWithNullTokenIsEnqueuedAtEnd()
        {
            DedupingQueue<DedupableInt> queue = new DedupingQueue<DedupableInt>();

            Assert.AreEqual(DedupingQueueAddResult.NewItem, queue.AddItem(new DedupableInt(3, "foo")));
            Assert.AreEqual(DedupingQueueAddResult.NewItem, queue.AddItem(new DedupableInt(4, null)));

            Assert.AreEqual(2, queue.Size);
            DedupableInt first;
            DedupableInt second;
            Assert.IsNotNull(queue.Dequeue(out first));
            Assert.IsNotNull(queue.Dequeue(out second));
            Assert.AreEqual(3, first.getVal());
            Assert.AreEqual(4, second.getVal());
        }

        [Test]
        public void QueueFullItemIsNotEnqueued()
        {
            DedupingQueue<DedupableInt> queue = new DedupingQueue<DedupableInt>(1);

            Assert.AreEqual(0, queue.Size);
            Assert.AreEqual(DedupingQueueAddResult.NewItem, queue.AddItem(new DedupableInt(3, "foo")));
            Assert.AreEqual(1, queue.Size);
            Assert.AreEqual(DedupingQueueAddResult.QueueFull, queue.AddItem(new DedupableInt(4, null)));
            Assert.AreEqual(1, queue.Size);
        }


        [Test]
        public void ItemWithMatchingTokenReplaces()
        {
            DedupingQueue<DedupableInt> queue = new DedupingQueue<DedupableInt>();

            Assert.AreEqual(DedupingQueueAddResult.NewItem, queue.AddItem(new DedupableInt(3, "foo")));
            Assert.AreEqual(DedupingQueueAddResult.NewItem, queue.AddItem(new DedupableInt(4, null)));
            Assert.AreEqual(DedupingQueueAddResult.Duplicate, queue.AddItem(new DedupableInt(5, "foo")));
            Assert.AreEqual(2, queue.Size);

            DedupableInt first;
            DedupableInt second;
            Assert.IsNotNull(queue.Dequeue(out first));
            Assert.IsNotNull(queue.Dequeue(out second));
            Assert.AreEqual(5, first.getVal());
            Assert.AreEqual(4, second.getVal());
        }

        [Test]
        public void IfNotAcceptedNotInDictionary()
        {
            DedupingQueue<DedupableInt> queue = new DedupingQueue<DedupableInt>(2);

            Assert.AreEqual(DedupingQueueAddResult.NewItem, queue.AddItem(new DedupableInt(3, "foo")));
            Assert.AreEqual(DedupingQueueAddResult.NewItem, queue.AddItem(new DedupableInt(4, null)));

            //This item will not be placed into the queue because the queue is full. This means that it should not be placed
            //in the Dictionary either becuase then after a matched id replace the object would STILL not be in the queue
            Assert.AreEqual(DedupingQueueAddResult.QueueFull, queue.AddItem(new DedupableInt(5, "bar")));

            Assert.AreEqual(2, queue.Size);

            DedupableInt first;
            Assert.IsTrue(queue.Dequeue(out first));
            Assert.AreEqual(3, first.getVal());

            Assert.AreEqual(DedupingQueueAddResult.NewItem, queue.AddItem(new DedupableInt(6, "bar")));

            DedupableInt second;
            DedupableInt third;
            Assert.IsTrue(queue.Dequeue(out second));
            Assert.AreEqual(4, second.getVal());
            Assert.IsTrue(queue.Dequeue(out third));
            Assert.AreEqual(6, third.getVal());
        }

        [Test]
        public void AfterDequeueNotInDictionary()
        {
            DedupingQueue<DedupableInt> queue = new DedupingQueue<DedupableInt>();

            Assert.AreEqual(DedupingQueueAddResult.NewItem, queue.AddItem(new DedupableInt(3, "foo")));
            DedupableInt item;
            queue.Dequeue(out item);

            //After the dequeue above the item is gone from the queue. If the item were still in the Dictionary then
            //queueing the same token would cause a replace for an item not in the queue
            Assert.AreEqual(DedupingQueueAddResult.NewItem, queue.AddItem(new DedupableInt(4, "foo")));
        }

        [Test]
        public void QueueWorksWithNonDedupableTypes()
        {
            DedupingQueue<int> queue = new DedupingQueue<int>(2);
            Assert.AreEqual(DedupingQueueAddResult.NewItem, queue.AddItem(1));
            Assert.AreEqual(DedupingQueueAddResult.NewItem, queue.AddItem(1));
            Assert.AreEqual(DedupingQueueAddResult.QueueFull, queue.AddItem(1));

            int dequeuedValue;
            Assert.IsTrue(queue.Dequeue(out dequeuedValue));
            Assert.AreEqual(1, dequeuedValue);
            Assert.IsTrue(queue.Dequeue(out dequeuedValue));
            Assert.AreEqual(1, dequeuedValue);
            Assert.IsFalse(queue.Dequeue(out dequeuedValue));
            Assert.AreEqual(0, dequeuedValue);
        }

        [Test]
        public void EnqueuingNullWorks()
        {
            DedupingQueue<object> queue = new DedupingQueue<object>();
            queue.AddItem(null);

            object dequeuedItem;
            Assert.IsTrue(queue.Dequeue(out dequeuedItem));
            Assert.IsNull(dequeuedItem);
            Assert.IsFalse(queue.Dequeue(out dequeuedItem));
            Assert.IsNull(dequeuedItem);
        }

    }
}
