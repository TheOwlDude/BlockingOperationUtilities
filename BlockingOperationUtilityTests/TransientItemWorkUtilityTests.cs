using BlockingOperationUtilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BlockingOperationUtilityTests
{
    public class ClassWithBlockingOperation<T>
    {
        public List<T> processdItems = new List<T>(); //items successfully  processed are added to the list
        public Exception throwMe = null;    //set this to cause BlockingOperation to throw.
        public bool unblock = false;        //client allows one item to pass by setting this true.

        public void BlockingOperation(T item)
        {
            while (!unblock) Thread.Sleep(10);
            unblock = false;

            Console.WriteLine("Dequeued {0}", item);
            if (throwMe != null)
            {
                Exception lclThrowMe = throwMe;
                throwMe = null;
                throw lclThrowMe;
            }

            processdItems.Add(item);
        }
    }

    public class TransientWorkItemUtilityWithErrorHandling<T> : TransientItemWorkUtility<T>
    {
        public Exception handlerThrows = null;

        public TransientWorkItemUtilityWithErrorHandling(Action<T> blockingOperation):base(blockingOperation, "TestThread")
        {}

        protected override void HandleOperationErrors(Exception e)
        {
            if (handlerThrows != null)
            {
                Exception lclHandlerThrows = handlerThrows;
                handlerThrows = null;
                throw lclHandlerThrows;
            }
        }
    }

    [TestFixture]
    public class TransientItemWorkUtilityTests
    {
        [Test]
        public void OperateUtility()
        {
            ClassWithBlockingOperation<int> classWithBlockingOp = new ClassWithBlockingOperation<int>();

            TransientWorkItemUtilityWithErrorHandling<int> utility =
                new TransientWorkItemUtilityWithErrorHandling<int>(classWithBlockingOp.BlockingOperation);

            utility.Add(1);
            utility.Add(2);
            utility.Add(3);
            utility.Add(4);

            utility.Start();

            classWithBlockingOp.unblock = true;
            Thread.Sleep(100);

            classWithBlockingOp.throwMe = new Exception("Blocking operation exception 1");
            classWithBlockingOp.unblock = true;
            Thread.Sleep(100);

            classWithBlockingOp.throwMe = new Exception("Blocking operation exception 2");
            utility.handlerThrows = new Exception("Exception handler exception");
            classWithBlockingOp.unblock = true;
            Thread.Sleep(100);

            classWithBlockingOp.unblock = true;
            Thread.Sleep(100);

            Assert.AreEqual(2, classWithBlockingOp.processdItems.Count);
            Assert.AreEqual(1, classWithBlockingOp.processdItems[0]);
            Assert.AreEqual(4, classWithBlockingOp.processdItems[1]);


        }
    }
}
