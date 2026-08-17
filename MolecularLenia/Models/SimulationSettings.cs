namespace MolecularLenia.Models
{
    public class SimulationSettings
    {
        public float InteractionRadius { get; set; } = 50f;
        public float AttractionForce { get; set; } = 0.5f;
        public float RepulsionForce { get; set; } = 1.0f;
        public bool IsPaused { get; set; } = false;
        public float SpeedMultiplier { get; set; } = 1.0f;
        public int TargetDensity { get; set; } = 6;         
        public float ReproductionRate { get; set; } = 1.0f; 
    }
}