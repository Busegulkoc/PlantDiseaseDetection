namespace FruitDiseaseDetection.Models
{
    public class Fruit
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Species { get; set; }
        public FruitVegetableDetails? FruitDetails { get; set; }
        public List<Disease>? Diseases { get; set; }
    }
}
