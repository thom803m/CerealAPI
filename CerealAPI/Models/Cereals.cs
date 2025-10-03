namespace CerealAPI.Models
{
    public class Cereal
    {
        public int Id { get; set; } // Primær nøgle
        public string Name { get; set; } // Produktnavn (f.eks. "100% Bran")
        public string Mfr { get; set; } // Producent. Står nu som "K" for Kellogg's, "G" for General Mills, "Q" for Quaker Oats, "R" for Ralston Purina og "A" for Nabisco
        public string Type { get; set; } // Type varmt (Hot) eller koldt (Cold). Det er "H" for Hot og "C" for Cold
        public int Calories { get; set; } 
        public int Protein { get; set; }
        public int Fat { get; set; }
        public int Sodium { get; set; }
        public float Fiber { get; set; }
        public float Carbo { get; set; }
        public int Sugars { get; set; }
        public int Potass { get; set; }
        public int Vitamins { get; set; }
        public int Shelf { get; set; } // Hylde i butik
        public float Weight { get; set; } // Vægt i gram
        public float Cups { get; set; } // Portion i kopper
        public double Rating { get; set; } // Bruger-rating 
        // public string? ImagePath { get; set; } 
    }
}
