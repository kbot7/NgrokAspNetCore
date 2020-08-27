using Microsoft.Extensions.Hosting;
using Ngrok.ApiClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ngrok.AspNetCore
{
	public interface INgrokHostedService : IHostedService
	{
		Task<IReadOnlyCollection<Tunnel>> GetTunnelsAsync();

		event Action<IReadOnlyCollection<Tunnel>> Ready;
	}
}