using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TerrariaLauncher.Commons.Database
{
    public class UnitOfWorkFactory : IUnitOfWorkFactory
    {
        private readonly string connectionString;
        public UnitOfWorkFactory(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public string ConnectionString => this.connectionString;

        public IUnitOfWork Create()
        {
            var connection = new MySql.Data.MySqlClient.MySqlConnection(this.connectionString);
            connection.Open();
            return new UnitOfWork(connection);
        }

        public async Task<IUnitOfWork> CreateAsync(CancellationToken cancellationToken = default)
        {
            var connection = new MySql.Data.MySqlClient.MySqlConnection(this.connectionString);
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            return new UnitOfWork(connection);
        }
    }
}
