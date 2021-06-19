using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TerrariaLauncher.Commons.Database
{
    class UnitOfWork : IUnitOfWork
    {
        public UnitOfWork(DbConnection connection)
        {
            this.Connection = connection;
        }

        public DbConnection Connection { get; private set; }
        public DbTransaction Transaction { get; private set; }
        public DbCommand CreateDbCommand(string commandText = "")
        {
            var dbCommand = this.Connection.CreateCommand();
            dbCommand.CommandText = commandText;
            dbCommand.Transaction = this.Transaction;
            return dbCommand;
        }

        public void Begin(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            this.Transaction = this.Connection.BeginTransaction(isolationLevel);
        }

        public async Task BeginAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, CancellationToken cancellationToken = default)
        {
            this.Transaction = await this.Connection.BeginTransactionAsync(isolationLevel, cancellationToken);
        }

        public void Commit()
        {
            this.Transaction.Commit();
        }

        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            await this.Transaction.CommitAsync(cancellationToken);
        }

        public void Rollback()
        {
            this.Transaction.Rollback();
        }

        public async Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            await this.Transaction.RollbackAsync(cancellationToken);
        }

        private bool disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed)
                return;

            if (disposing)
            {
                if (this.Transaction is not null)
                {
                    this.Transaction.Dispose();
                }
                this.Connection.Dispose();
            }

            this.Transaction = null;
            this.Connection = null;
            this.disposed = true;
        }

        protected virtual async ValueTask DisposeAsyncCore(bool disposing)
        {
            if (this.disposed)
                return;

            if (disposing)
            {
                if (this.Transaction is not null)
                {
                    await this.Transaction.DisposeAsync().ConfigureAwait(false);
                }
                await this.Connection.DisposeAsync().ConfigureAwait(false);
            }

            this.Transaction = null;
            this.Connection = null;
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            await this.DisposeAsyncCore(false).ConfigureAwait(false);
            this.Dispose(disposing: false);
            GC.SuppressFinalize(this);
        }
    }
}
