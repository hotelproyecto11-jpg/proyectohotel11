using System.Net.Http.Json;
using PricingMvp.Application.DTOs;

namespace PricingMvp.Infrastructure.Services
{
    public class MlPredictRequest
    {
        public int roomId { get; set; }
        public decimal basePrice { get; set; }
        public double hotelOccupancy { get; set; }
        public int dayOfWeek { get; set; }
        public int month { get; set; }
        public bool isWeekend { get; set; }
        public int? capacity { get; set; }
        public bool? hasSeaView { get; set; }
    }

    public class MlPredictResponse
    {
        public double predictedPrice { get; set; }
        public string modelVersion { get; set; } = string.Empty;
    }

    public class MlPricingClient
    {
        private readonly HttpClient _http;

        public MlPricingClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<MlPredictResponse?> PredictAsync(MlPredictRequest req)
        {
            try
            {
                var resp = await _http.PostAsJsonAsync("/predict", req);
                if (!resp.IsSuccessStatusCode) return null;
                var body = await resp.Content.ReadFromJsonAsync<MlPredictResponse>();
                return body;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> TrainAsync(IEnumerable<object> rows)
        {
            try
            {
                var payload = new { rows = rows };
                var resp = await _http.PostAsJsonAsync("/train", payload);
                return resp.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}
