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
            // Log request details to help diagnose the issue
            var headers = Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString());
            var formFiles = Request.Form.Files.Select(f => f.Name).ToList();
            
            Console.WriteLine($"Received request with {Request.Form.Files.Count} files");
            Console.WriteLine($"Form file names: {string.Join(", ", formFiles)}");
            
            if (imageFile == null)
            {
                // Try to get the file from the request if the parameter binding didn't work
                if (Request.Form.Files.Count > 0)
                {
                    imageFile = Request.Form.Files[0];
                    Console.WriteLine($"Found file with name {imageFile.Name} in request");
                }
                else
                {
                    return BadRequest(new { error = "No image file was received. Please ensure you're sending a file with the name 'imageFile'." });
                }
            }

            if (imageFile.Length == 0)
            {
                return BadRequest(new { error = "The uploaded file is empty!" });
            }

            try
            {
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
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}", details = ex.StackTrace });
            }
        }

        // Add an alternative endpoint that doesn't rely on model binding
        [HttpPost("upload")]
        public async Task<ActionResult<object>> Upload()
        {
            try
            {
                if (!Request.HasFormContentType || Request.Form.Files.Count == 0)
                {
                    return BadRequest(new { error = "No files were uploaded or content type is incorrect. Please send a multipart/form-data request with a file." });
                }

                var file = Request.Form.Files[0];
                
                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);

                    var uploadedImage = new UploadedImage
                    {
                        FileName = file.FileName,
                        ImageData = memoryStream.ToArray(),
                        UploadDate = DateTime.UtcNow
                    };

                    _context.UploadedImages.Add(uploadedImage);
                    await _context.SaveChangesAsync();

                    var result = await CallPredictionModel(uploadedImage);
                    
                    return Ok(new
                    {
                        Prediction = result
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}", details = ex.StackTrace });
            }
        }

        private async Task<PredictionResult> CallPredictionModel(UploadedImage image)
        {
            try
            {
                var predictionResult = await _predictionService.PredictAsync(image.ImageData, image.FileName);
                
                // Check if we got an error from the prediction service
                if (!string.IsNullOrEmpty(predictionResult.Error))
                {
                    Console.WriteLine($"Prediction error: {predictionResult.Error}");
                }
                else if (predictionResult.Predictions?.Count == 0)
                {
                    Console.WriteLine("No predictions were returned from the model");
                }
                else
                {
                    Console.WriteLine($"Received {predictionResult.Predictions?.Count} predictions");
                }
                
                return predictionResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in CallPredictionModel: {ex.Message}");
                return new PredictionResult { Error = ex.Message };
            }
        }
    }
}
