using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NGrok.ApiClient
{
	public interface INGrokApiClient
	{
		// GET /api/tunnels 
		Task<IEnumerable<Tunnel>> ListTunnelsAsync(CancellationToken cancellationToken = default);

		// POST /api/tunnels 
		Task<Tunnel> StartTunnelAsync(StartTunnelRequest request, CancellationToken cancellationToken = default);

		// GET /api/tunnels/:name 
		Task<Tunnel> GetTunnelAsync(string name, CancellationToken cancellationToken = default);

		// DELETE /api/tunnels/:name 
		Task StopTunnelAsync(string name, CancellationToken cancellationToken = default);

		Task<bool> CheckIfLocalAPIUpAsync();

	}
}