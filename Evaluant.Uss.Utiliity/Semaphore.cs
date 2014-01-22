using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Evaluant.Uss.Utility
{
    public class Semaphore
    {
        int count = 0;

        object countLock = new object();

        public void AddOne()
        {
            lock (countLock)
            {
                count++;
            }
        }

        public void RemoveOne()
        {
            lock (countLock)
            {
                count--;
            }
        }

        public bool HasOnePending
        {
            get
            {
                lock (countLock)
                { return count != 0; }
            }
        }


    }
}
