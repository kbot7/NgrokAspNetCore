using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace FluffySpoon.AspNet.NGrok
{
    internal interface INGrokHostedService : IHostedService
    {
        event Action Ready;
    }
}