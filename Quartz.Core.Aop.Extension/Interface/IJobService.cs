using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Quartz.Core.Aop.Extension.Interface
{
    public interface IJobService : IHostedService, IDisposable
    {
        Task PuaseScheduler(CancellationToken cancellationToken);

    }
}