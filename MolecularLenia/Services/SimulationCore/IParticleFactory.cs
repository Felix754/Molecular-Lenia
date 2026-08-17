using MolecularLenia.Models;
using System;

namespace MolecularLenia.Services
{
    public interface IParticleFactory
    {
        Particle CreateRandom();
        Particle Create(float x, float y, MoleculeType type);
        Particle CreateChild(Particle parent);
        void MutateToPathogen(Particle p);
    }

    public class ParticleFactory : IParticleFactory
    {
        private readonly Random _rand = new();

        public Particle CreateRandom()
        {
            float x = (float)(_rand.NextDouble() * 1800 + 60);
            float y = (float)(_rand.NextDouble() * 960 + 60);
            MoleculeType randomType = (MoleculeType)_rand.Next(0, 4);
            return Create(x, y, randomType);
        }

        public Particle Create(float x, float y, MoleculeType type)
        {
            return new Particle
            {
                X = x,
                Y = y,
                Type = type
            };
        }

        public Particle CreateChild(Particle parent)
        {
            float x = parent.X + (float)(_rand.NextDouble() - 0.5) * 12;
            float y = parent.Y + (float)(_rand.NextDouble() - 0.5) * 12;
            return Create(x, y, parent.Type);
        }

        public void MutateToPathogen(Particle p)
        {
            p.Type = MoleculeType.Pathogen;
        }
    }
}