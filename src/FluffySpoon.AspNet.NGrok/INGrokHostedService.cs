using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using NGrok.ApiClient;

namespace FluffySpoon.AspNet.NGrok
{
    public interface INGrokHostedService : IHostedService
    {
        Task<IReadOnlyCollection<Tunnel>> GetTunnelsAsync();

		event Action<IEnumerable<Tunnel>> Ready;
	}
}