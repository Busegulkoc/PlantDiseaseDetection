using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FruitDiseaseDetection.Data;
using FruitDiseaseDetection.Models;
using FruitDiseaseDetection.Services;
using System.Text.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.IO;
using System;

namespace FruitDiseaseDetection.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly FruitDbContext _context;
        private readonly PredictionService _predictionService;

        public UploadController(FruitDbContext context, PredictionService predictionService)
        {
            _context = context;
            _predictionService = predictionService;
        }

        [HttpPost("upload-image")]
        public async Task<ActionResult<Fruit>> UploadImage([FromForm] IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                return BadRequest("Image could not be loaded!");
            }

            using (var memoryStream = new MemoryStream())
            {
                await imageFile.CopyToAsync(memoryStream);

                var uploadedImage = new UploadedImage
                {
                    FileName = imageFile.FileName,
                    ImageData = memoryStream.ToArray(),
                    UploadDate = DateTime.UtcNow
                };

                _context.UploadedImages.Add(uploadedImage);
                await _context.SaveChangesAsync();

                // Model API'ye gönder
                var result = await CallPredictionModel(uploadedImage);
                
                return Ok(new
                {
                    Prediction = result
                });
            }
        }

        private async Task<object> CallPredictionModel(UploadedImage image)
        {
            try
            {
                var predictionResult = await _predictionService.PredictAsync(image.ImageData, image.FileName);
                return predictionResult;
            }
            catch (Exception ex)
            {
                return new { error = ex.Message };
            }
        }
    }
}
