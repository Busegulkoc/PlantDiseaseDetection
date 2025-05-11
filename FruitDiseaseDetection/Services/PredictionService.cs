using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.IO;
using System;
using System.Text.Json;
using System.Collections.Generic;

namespace FruitDiseaseDetection.Services
{
    public class PredictionService
    {
        private readonly HttpClient _httpClient;

        public PredictionService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<PredictionResult> PredictAsync(byte[] imageBytes, string fileName)
        {
            try
            {
                using (var content = new MultipartFormDataContent())
                {
                    var fileContent = new ByteArrayContent(imageBytes);
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                    // Change parameter name from 'file' to match what your API expects
                    content.Add(fileContent, "file", fileName);  // Keep it as 'file' to match the FastAPI endpoint

                    // Get the API URL from environment variable or use default
                    var apiUrl = "https://206e-176-220-163-163.ngrok-free.app";
                    var response = await _httpClient.PostAsync($"{apiUrl}/predict", content);
                    response.EnsureSuccessStatusCode();
                    var result = await response.Content.ReadAsStringAsync();

                    // Parse the JSON response
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var predictionResult = JsonSerializer.Deserialize<PredictionResult>(result, options);
                    
                    return predictionResult ?? new PredictionResult 
                    { 
                        Error = "Failed to parse prediction result" 
                    };
                }
            }
            catch (Exception ex)
            {
                return new PredictionResult { Error = ex.Message };
            }
        }
    }

    public class PredictionResult
    {
        public List<PredictionItem> Predictions { get; set; } = new List<PredictionItem>();
        public string Error { get; set; }
    }

    public class PredictionItem
    {
        public string Class { get; set; }
        public float Probability { get; set; }
    }
}
