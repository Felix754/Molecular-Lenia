using MolecularLenia.Models;

namespace MolecularLenia.Services
{
    public interface ISimulationSettingsManager
    {
        SimulationSettings GetSettings();
        void UpdateSettings(SimulationSettings newSettings);
        void ResetSettings();
    }

    public class SimulationSettingsManager : ISimulationSettingsManager
    {
        private SimulationSettings _settings = new();

        public SimulationSettings GetSettings() => _settings;

        public void UpdateSettings(SimulationSettings newSettings)
        {
            _settings.InteractionRadius = newSettings.InteractionRadius;
            _settings.AttractionForce = newSettings.AttractionForce;
            _settings.RepulsionForce = newSettings.RepulsionForce;
            _settings.IsPaused = newSettings.IsPaused;
            _settings.SpeedMultiplier = newSettings.SpeedMultiplier;
            _settings.TargetDensity = newSettings.TargetDensity;
            _settings.ReproductionRate = newSettings.ReproductionRate;
        }

        public void ResetSettings() => _settings = new SimulationSettings();
    }
}