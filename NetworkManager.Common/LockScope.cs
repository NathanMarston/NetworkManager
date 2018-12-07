using System;
using System.Threading;

namespace NetworkManager.Common
{
    /// <summary>
    /// Extension methods for ReaderWriterLockSlim - to support read and write locking scopes
    /// </summary>
    public static class ReaderWriterExt
    {
        /// <summary>
        /// Disposable Reader Lock Scope
        /// </summary>
        sealed class ReadLockScope : IDisposable
        {
            private ReaderWriterLockSlim sync;

            public ReadLockScope(ReaderWriterLockSlim sync)
            {
                this.sync = sync;
                sync.EnterReadLock();
            }

            public void Dispose()
            {
                if (sync != null)
                {
                    sync.ExitReadLock();
                    sync = null;
                }
            }
        }

        /// <summary>
        /// Extension method for getting a read lock scope
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static IDisposable Read(this ReaderWriterLockSlim obj)
        {
            return new ReadLockScope(obj);
        }

        /// <summary>
        /// Disposable write lock scope 
        /// </summary>
        sealed class WriteLockToken : IDisposable
        {
            private ReaderWriterLockSlim sync;

            public WriteLockToken(ReaderWriterLockSlim sync)
            {
                this.sync = sync;
                sync.EnterWriteLock();
            }

            public void Dispose()
            {
                if (sync != null)
                {
                    sync.ExitWriteLock();
                    sync = null;
                }
            }
        }

        /// <summary>
        /// Extension method for getting a write lock scope
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static IDisposable Write(this ReaderWriterLockSlim obj)
        {
            return new WriteLockToken(obj);
        }
    }
}
