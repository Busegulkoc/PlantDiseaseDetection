namespace FruitDiseaseDetection.Models
{
    public class ImageResult
    {
        public int Id { get; set; }             
        public int? ImageId { get; set; }  
        public int? DetectedDiseaseId { get; set; }
        public Disease? DetectedDisease { get; set; }
        public string? ModelLabel { get; set; }    
        public float? Confidence { get; set; }    
        public DateTime? AnalysisDate { get; set; } 
    }
}
