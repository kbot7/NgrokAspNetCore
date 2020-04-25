using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Ngrok.ApiClient;

namespace Ngrok.AspNetCore
{
	public interface INgrokHostedService : IHostedService
	{
		Task<IReadOnlyCollection<Tunnel>> GetTunnelsAsync();

		event Action<IReadOnlyCollection<Tunnel>> Ready;
	}
}