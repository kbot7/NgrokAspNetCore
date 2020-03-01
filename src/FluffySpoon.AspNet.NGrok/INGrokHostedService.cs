using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluffySpoon.AspNet.NGrok.NGrokModels;
using Microsoft.Extensions.Hosting;

namespace FluffySpoon.AspNet.NGrok
{
    public interface INGrokHostedService : IHostedService
    {
        Task<IEnumerable<Tunnel>> GetTunnelsAsync();

        event Action<IEnumerable<Tunnel>> Ready;
    }
}