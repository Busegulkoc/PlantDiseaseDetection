using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FruitDiseaseDetection.Models
{
    public class UploadedImage
    {
        public int Id { get; set; }               
        public int? UserId { get; set; }          
        public User? User { get; set; }
        public int? FruitVegetableId { get; set; }         
        public Fruit? FruitVegetable { get; set; }
        public string? FileName { get; set; }       
        public string? ImagePath { get; set; }     
        public byte[]? ImageData { get; set; }      
        public DateTime? UploadDate { get; set; }   
        public ImageResult? ImageResult { get; set; }  


    }
}
