namespace MolecularLenia.Models
{
    public class Particle
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Vx { get; set; }
        public float Vy { get; set; }
        public MoleculeType Type { get; set; }
        public float Health { get; set; } = 100f;
        public float AgeInIdealZone { get; set; } = 0f;
    }
}