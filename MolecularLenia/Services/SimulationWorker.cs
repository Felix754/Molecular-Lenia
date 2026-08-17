using MolecularLenia.Hubs;
using MolecularLenia.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace MolecularLenia.Services
{
    public class SimulationWorker : BackgroundService
    {
        private readonly SimulationService _engine;
        private readonly IHubContext<SimulationHub> _hubContext;

        public SimulationWorker(SimulationService engine, IHubContext<SimulationHub> hubContext)
        {
            _engine = engine;
            _hubContext = hubContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await _engine.UpdateFrameAsync();

                var snapshot = await _engine.GetParticlesSnapshotAsync();
                var settings = _engine.GetSettings();

                // Send current frame state and updated settings to each connected client
                await _hubContext.Clients.All.SendAsync("RenderFrame", snapshot, settings, stoppingToken);

                await Task.Delay(33, stoppingToken);
            }
        }
    }
}