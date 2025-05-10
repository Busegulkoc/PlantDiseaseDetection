using System.Text.Json.Serialization;

namespace FruitDiseaseDetection.Models
{
    public class Disease
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? CommonName { get; set; }
        public FruitVegetableDetails? DiseaseDetails { get; set; }
        public string? TreatmentSuggestion { get; set; }
        public string? Symptoms { get; set; }
        [JsonIgnore]
        public List<Fruit>? FruitsVegetables { get; set; }

    }
}
