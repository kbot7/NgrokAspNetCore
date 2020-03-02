using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NgrokAspNetCore.Lib.NgrokModels;
using Microsoft.Extensions.Hosting;

namespace NgrokAspNetCore.Lib
{
    public interface INgrokHostedService : IHostedService
    {
        Task<IEnumerable<Tunnel>> GetTunnelsAsync();

        event Action<IEnumerable<Tunnel>> Ready;
    }
}