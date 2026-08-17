using MolecularLenia.Models;
using System;

namespace MolecularLenia.Services
{
    public interface IPhysicsEngine
    {
        void AccumulateForce(Particle p1, Particle p2, float dist, float dx, float dy, SimulationSettings settings, ref float fx, ref float fy);
        void ApplyKinematics(Particle p, float fx, float fy, float dt);
    }

    public class PhysicsEngine : IPhysicsEngine
    {
        private readonly float[,] _attractionMatrix = new float[5, 5]
        {
            {  0.5f,  1.2f, -0.6f,  1.5f, -1.0f }, 
            { -0.2f,  0.6f,  1.1f,  1.5f, -1.0f }, 
            {  0.9f, -0.7f,  0.4f,  1.5f, -1.0f }, 
            {  0.0f,  0.0f,  0.0f,  0.0f,  0.0f }, 
            {  1.5f,  1.5f,  1.5f,  0.5f,  0.2f }  
        };

        public void AccumulateForce(Particle p1, Particle p2, float dist, float dx, float dy, SimulationSettings settings, ref float fx, ref float fy)
        {
            float rMax = settings.InteractionRadius;
            float rCore = rMax * 0.3f;
            float force = 0;
            float matrixVal = _attractionMatrix[(int)p1.Type, (int)p2.Type];

            if (dist < rCore)
            {
                force = -settings.RepulsionForce * (1.0f - dist / rCore);
            }
            else
            {
                float normArea = (dist - rCore) / (rMax - rCore);
                float bellShape = 1.0f - MathF.Abs(2.0f * normArea - 1.0f);
                force = settings.AttractionForce * matrixVal * bellShape;
            }

            fx += (dx / dist) * force;
            fy += (dy / dist) * force;
        }

        public void ApplyKinematics(Particle p, float fx, float fy, float dt)
        {
            if ((int)p.Type == 3) return; // Fuel static

            p.Vx = (p.Vx + fx) * 0.82f;
            p.Vy = (p.Vy + fy) * 0.82f;
            p.X += p.Vx * dt;
            p.Y += p.Vy * dt;

            p.X = Math.Clamp(p.X, 15, 1905);
            p.Y = Math.Clamp(p.Y, 15, 1065);
        }
    }
}