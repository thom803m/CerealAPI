namespace CerealAPI.Models
{
    public class Cereal
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Mfr { get; set; }
        public string Type { get; set; }
        public int Calories { get; set; }
        public int Protein { get; set; }
        public int Fat { get; set; }
        public int Sodium { get; set; }
        public double Fiber { get; set; }
        public double Carbo { get; set; }
        public int Sugars { get; set; }
        public int Potass { get; set; }
        public int Vitamins { get; set; }
        public int Shelf { get; set; }
        public double Weight { get; set; }
        public double Cups { get; set; }
        public double Rating { get; set; }
        public string? ImagePath { get; set; }
    }
}