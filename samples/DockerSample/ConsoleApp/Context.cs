using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp
{
    public class CustomSynchronizationContext : SynchronizationContext
    {
        private readonly BlockingCollection<Tuple<SendOrPostCallback, object>> _queue = new BlockingCollection<Tuple<SendOrPostCallback, object>>();

        public override void Post(SendOrPostCallback d, object state)
        {
            _queue.Add(Tuple.Create(d, state));
        }

        public override void Send(SendOrPostCallback d, object state)
        {
            throw new NotSupportedException("Send is not supported in this context.");
        }

        public void RunOnCurrentThread()
        {
            foreach (var workItem in _queue.GetConsumingEnumerable())
            {
                workItem.Item1(workItem.Item2);
            }
        }

        public void Complete()
        {
            _queue.CompleteAdding();
        }
    }
}
