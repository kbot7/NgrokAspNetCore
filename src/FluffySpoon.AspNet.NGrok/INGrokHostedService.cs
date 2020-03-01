using System;
using System.Threading;
using System.Threading.Tasks;
using FluffySpoon.AspNet.NGrok.NGrokModels;
using Microsoft.Extensions.Hosting;

namespace FluffySpoon.AspNet.NGrok
{
    public interface INGrokHostedService : IHostedService
    {
        Tunnel[] Tunnels { get; }

        event Action Ready;
    }
}