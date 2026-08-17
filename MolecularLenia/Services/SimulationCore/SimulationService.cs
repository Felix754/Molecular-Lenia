using MolecularLenia.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MolecularLenia.Services
{
    public class SimulationService
    {
        private readonly List<Particle> _particles = new();
        private readonly ISimulationSettingsManager _settingsManager;
        private readonly IParticleFactory _particleFactory;
        private readonly IPhysicsEngine _physics;
        private readonly IBiologyEngine _biology;
        private readonly SpatialGrid _spatialGrid = new();

        public SimulationService(
            ISimulationSettingsManager settingsManager,
            IParticleFactory particleFactory,
            IPhysicsEngine physics,
            IBiologyEngine biology)
        {
            _settingsManager = settingsManager;
            _particleFactory = particleFactory;
            _physics = physics;
            _biology = biology;

            InitRandomParticles(250);
        }

        public void InitRandomParticles(int count)
        {
            lock (_particles)
            {
                for (int i = 0; i < count; i++)
                {
                    _particles.Add(_particleFactory.CreateRandom());
                }
            }
        }

        // Delegate settings management
        public SimulationSettings GetSettings() => _settingsManager.GetSettings();
        public void UpdateSettings(SimulationSettings s) => _settingsManager.UpdateSettings(s);
        public void ResetSettings() => _settingsManager.ResetSettings();

        public void SpawnParticle(float x, float y, MoleculeType type)
        {
            lock (_particles) { _particles.Add(_particleFactory.Create(x, y, type)); }
        }

        public void Clear()
        {
            lock (_particles) { _particles.Clear(); }
        }

        public Task<List<Particle>> GetParticlesSnapshotAsync()
        {
            lock (_particles) { return Task.FromResult(new List<Particle>(_particles)); }
        }

        public Task UpdateFrameAsync()
        {
            lock (_particles)
            {
                var settings = _settingsManager.GetSettings();
                if (settings.IsPaused) return Task.CompletedTask;

                float dt = 0.16f * settings.SpeedMultiplier;
                float rMax = settings.InteractionRadius;

                // Spatial optimization (reusing spatial grid)
                _spatialGrid.Build(_particles, rMax);

                List<Particle> newParticles = new();

                // Main game loop
                for (int i = _particles.Count - 1; i >= 0; i--)
                {
                    var p1 = _particles[i];
                    float fx = 0, fy = 0;
                    int neighborsCount = 0;

                    // Replace these three lines in your loop
                    int pCellX = Math.Clamp((int)(p1.X / _spatialGrid.CellSize), 0, _spatialGrid.Cols - 1);
                    int pCellY = Math.Clamp((int)(p1.Y / _spatialGrid.CellSize), 0, _spatialGrid.Rows - 1);

                    for (int cx = Math.Max(0, pCellX - 1); cx <= Math.Min(_spatialGrid.Cols - 1, pCellX + 1); cx++)
                    {
                        for (int cy = Math.Max(0, pCellY - 1); cy <= Math.Min(_spatialGrid.Rows - 1, pCellY + 1); cy++)
                        {
                            var cell = _spatialGrid.Cells[cx, cy];
                            // ...
                            for (int j = 0; j < cell.Count; j++)
                            {
                                var p2 = cell[j];
                                if (p1 == p2) continue;

                                float dx = p2.X - p1.X;
                                float dy = p2.Y - p1.Y;
                                float dist = MathF.Sqrt(dx * dx + dy * dy);

                                if (dist > 0 && dist < rMax)
                                {
                                    neighborsCount++;

                                    // Biological interactions (Infection, Fuel)
                                    _biology.ProcessInteractions(p1, p2, dist, settings, _particleFactory);

                                    // Physics interactions (Attraction, Repulsion)
                                    _physics.AccumulateForce(p1, p2, dist, dx, dy, settings, ref fx, ref fy);
                                }
                            }
                        }
                    }

                    // Movement
                    _physics.ApplyKinematics(p1, fx, fy, dt);

                    // Lifecycle (Aging, Reproduction, Death)
                    bool isDead = _biology.ProcessLifecycle(p1, neighborsCount, _particles.Count, settings, _particleFactory, out Particle? child);

                    if (child != null) newParticles.Add(child);
                    if (isDead) _particles.RemoveAt(i);
                }

                _particles.AddRange(newParticles);
            }

            return Task.CompletedTask;
        }
    }
}