using Microsoft.AspNetCore.SignalR;
using MolecularLenia.Models;
using MolecularLenia.Services;

namespace MolecularLenia.Hubs
{
    public class SimulationHub : Hub
    {
        private readonly SimulationService _service;

        public SimulationHub(SimulationService service)
        {
            _service = service;
        }

        public void UpdateSettings(SimulationSettings settings)
        {
            _service.UpdateSettings(settings);
        }

        public void ResetSettings()
        {
            _service.ResetSettings();
        }

        public void Clear()
        {
            _service.Clear();
        }

        public void SpawnParticle(double x, double y, int type)
        {
            _service.SpawnParticle((float)x, (float)y, (MoleculeType)type);
        }

        public void SpawnBatch(int count)
        {
            _service.InitRandomParticles(count);
        }
    }
}