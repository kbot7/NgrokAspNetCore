using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ngrok.ApiClient
{
	public interface INgrokApiClient
	{
		// GET /api/tunnels 
		Task<IEnumerable<Tunnel>> ListTunnelsAsync(CancellationToken cancellationToken);

		// POST /api/tunnels 
		Task<Tunnel> StartTunnelAsync(StartTunnelRequest request, CancellationToken cancellationToken);

		// GET /api/tunnels/:name 
		Task<Tunnel> GetTunnelAsync(string name, CancellationToken cancellationToken);

		// DELETE /api/tunnels/:name 
		Task StopTunnelAsync(string name, CancellationToken cancellationToken);

	}
}
