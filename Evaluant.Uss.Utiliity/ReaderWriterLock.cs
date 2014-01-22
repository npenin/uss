using System;
using System.Threading;

namespace Evaluant.Uss.Utility
{
    public class ReaderWriterLock
    {
        private ManualResetEvent writerLock = new ManualResetEvent(true);

        private Semaphore readerLock = new Semaphore();

        public void AcquireReaderLock()
        {
            lock (readerLock)
            {
                if (!readerLock.HasOnePending)
                    writerLock.WaitOne();
                readerLock.AddOne();
            }
        }

        public void ReleaseReaderLock()
        {
            lock (readerLock)
            {
                readerLock.RemoveOne();
                if (!readerLock.HasOnePending)
                    writerLock.Set();
            }
        }

        public void AcquireWriterLock()
        {
            writerLock.WaitOne();
        }

        public void ReleaseWriterLock()
        {
            writerLock.Set();
        }
    }
}
