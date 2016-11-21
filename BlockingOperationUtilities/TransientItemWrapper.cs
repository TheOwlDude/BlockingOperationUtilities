using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockingOperationUtilities
{
    public class TransientItemWrapper<T>
    {
        public T Item { get; set; }
        public TransientItemWrapper(T item)
        {
            Item = item;
        }
    }
}
