using MolecularLenia.Models;
using System;

namespace MolecularLenia.Services
{
    public interface IBiologyEngine
    {
        void ProcessInteractions(Particle p1, Particle p2, float dist, SimulationSettings settings, IParticleFactory factory);

        // nullable reference type for Particle child to indicate that it can be null if no child is created
        bool ProcessLifecycle(Particle p, int neighborsCount, int totalParticles, SimulationSettings settings, IParticleFactory factory, out Particle? child);
    }

    public class BiologyEngine : IBiologyEngine
    {
        public void ProcessInteractions(Particle p1, Particle p2, float dist, SimulationSettings settings, IParticleFactory factory)
        {
            float rCore = settings.InteractionRadius * 0.3f;

            // Pathogen infection
            if ((int)p1.Type == 4 && (int)p2.Type <= 2 && dist < rCore * 1.2f)
            {
                factory.MutateToPathogen(p2);
            }

            // Fuel consumption
            if ((int)p2.Type == 3 && (int)p1.Type != 3 && dist < rCore * 1.5f)
            {
                p1.Health = MathF.Min(100f, p1.Health + 2.0f);
                p2.Health -= 2.5f;
            }
        }

        // nullable reference type for Particle as previously mentioned
        public bool ProcessLifecycle(Particle p, int neighborsCount, int totalParticles, SimulationSettings settings, IParticleFactory factory, out Particle? child)
        {
            child = null; // Prevents compiler warnings for unassigned out parameters

            int minLive = Math.Max(1, (int)(settings.TargetDensity * 0.3f));
            int maxLive = (int)(settings.TargetDensity * 2.2f);
            int minBreed = Math.Max(2, (int)(settings.TargetDensity * 0.6f));
            int maxBreed = (int)(settings.TargetDensity * 1.4f);

            if (neighborsCount < minLive || neighborsCount > maxLive)
            {
                p.Health -= 1.2f;
                p.AgeInIdealZone = 0;
            }
            else if (neighborsCount >= minBreed && neighborsCount <= maxBreed)
            {
                p.Health = MathF.Min(100f, p.Health + 0.6f);
                p.AgeInIdealZone += settings.ReproductionRate;

                if (p.AgeInIdealZone >= 60f && totalParticles < 1200 && (int)p.Type != 3)
                {
                    p.AgeInIdealZone = 0;
                    child = factory.CreateChild(p);
                }
            }

            return p.Health <= 0; // If true, the particle has died and will be removed
        }
    }
}