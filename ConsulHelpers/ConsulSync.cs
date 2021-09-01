using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TerrariaLauncher.Commons.Consul;
using TerrariaLauncher.Commons.Consul.API.EndPoints.Agent.Services.Queries.GetServiceConfiguration;

namespace TerrariaLauncher.Commons.ConsulHelpers
{
    public class ConsulSync: IDisposable
    {
        HttpClient httpClient;

        public ConsulSync(ConsulHostConfiguration config)
        {
            this.httpClient = new HttpClient();

            var schema = config.UseTls ? "https" : "http";
            this.httpClient.BaseAddress = new Uri($"{schema}://{config.Host}:{config.Port}/v1/");
        }

        public (string Address, int Port) GetServiceEndPoint(string serviceId)
        {
            return this.GetServiceEndPointAsync(serviceId).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        private async Task<(string Address, int Port)> GetServiceEndPointAsync(string serviceId)
        {
            var handler = new GetServiceConfigurationHandler();
            var query = new GetServiceConfigurationQuery()
            {
                ServiceId = serviceId
            };
            var result = await handler.Handle(this.httpClient, query).ConfigureAwait(false);

            return (result.Service.Address, result.Service.Port);
        }

        private bool disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }
            
            disposed = true;
            if (disposing)
            {
                this.httpClient.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
