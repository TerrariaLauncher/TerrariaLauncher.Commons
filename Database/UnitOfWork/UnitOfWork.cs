﻿using System;
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

        public T RunQueryHandler<T>(IQuerySingleHandler<T> queryHandler)
        {
            return queryHandler.Handle(this.Connection, this.Transaction);
        }

        public IEnumerable<T> RunQueryHandler<T>(IQueryHandler<T> queryHandler)
        {
            return queryHandler.Handle(this.Connection, this.Transaction);
        }

        public Task<IEnumerable<T>> RunQueryHandler<T>(IQueryHandlerAsync<T> queryHandler, CancellationToken cancellationToken = default)
        {
            return queryHandler.HandleAsync(this.Connection, this.Transaction, cancellationToken);
        }

        public IAsyncEnumerable<T> RunQueryHandler<T>(IQueryHandlerAsyncEnumerable<T> queryHandler, CancellationToken cancellationToken = default)
        {
            return queryHandler.HandleAsync(this.Connection, this.Transaction, cancellationToken);
        }

        public Task<T> RunQueryHandler<T>(IQuerySingleHandlerAsync<T> queryHandler, CancellationToken cancellationToken = default)
        {
            return queryHandler.HandleAsync(this.Connection, this.Transaction, cancellationToken);
        }

        public void RunCommandHandler(ICommandHandler commandHandler)
        {
            if (commandHandler.RequiredTransaction && this.Transaction is null)
            {
                throw new InvalidOperationException($"The command handler {commandHandler.GetType()} requires a transaction.");
            }
            
            commandHandler.Handle(this.Connection, this.Transaction);
        }

        public T RunCommandHandler<T>(ICommandHandler<T> commandHandler)
        {
            if (commandHandler.RequiredTransaction && this.Transaction is null)
            {
                throw new InvalidOperationException($"The command handler {commandHandler.GetType()} requires a transaction.");
            }

            return commandHandler.Handle(this.Connection, this.Transaction);
        }

        public Task RunCommandHandler(ICommandHandlerAsync commandHandler, CancellationToken cancellationToken = default)
        {
            if (commandHandler.RequiredTransaction && this.Transaction is null)
            {
                throw new InvalidOperationException($"The command handler {commandHandler.GetType()} requires a transaction.");
            }
            return commandHandler.Handle(this.Connection, this.Transaction, cancellationToken);
        }

        public Task<T> RunCommandHandler<T>(ICommandHandlerAsync<T> commandHandler, CancellationToken cancellationToken = default)
        {
            if (commandHandler.RequiredTransaction && this.Transaction is null)
            {
                throw new InvalidOperationException($"The command handler {commandHandler.GetType()} requires a transaction.");
            }
            return commandHandler.Handle(this.Connection, this.Transaction, cancellationToken);
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