using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace temperaturemois.BackgroundService
{
    public abstract class BackgroundService : IHostedService, IDisposable
    {
        private Task _executingTask;
        private readonly CancellationTokenSource _stoppingCts = new CancellationTokenSource();

        protected abstract Task ExecuteAsync(CancellationToken stoppingToken);

        public virtual Task StartAsync(CancellationToken cancellationToken)
        {
            _executingTask = ExecuteAsync(_stoppingCts.Token);

            if (_executingTask.IsCompleted)
            {
                return _executingTask;
            }

            return Task.CompletedTask;
        }

        public virtual async Task StopAsync(CancellationToken cancellationToken)
        {
            
                await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken));
            
        }

        public virtual void Dispose()
        {
            _stoppingCts.Cancel();
        }
    }
}
