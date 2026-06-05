using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using SmartIndustrialRecruitment.Abstractions;
using SmartIndustrialRecruitment.Contracts.Recommendation;

namespace SmartIndustrialRecruitment.Services.Recommendation;

public class RecommendationService : IRecommendationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<RecommendationService> _logger;

    // رابط الـ Python AI API (Flask/FastAPI)
    // يُعرَّف في appsettings.json
    private const string PythonApiUrl = "http://localhost:5000/recommend";

    public RecommendationService(HttpClient httpClient, ILogger<RecommendationService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<Result<RecommendationResponse>> GetRecommendedJobsAsync(RecommendationRequest request)
    {
        try
        {
            // تجهيز الـ Request للـ Python API
            var pythonRequest = new
            {
                worker_job_type = request.WorkerJobType,
                worker_location = request.WorkerLocation,
                worker_experience = request.WorkerExperience
            };

            var json = JsonSerializer.Serialize(pythonRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // إرسال الـ Request للـ Python API
            var response = await _httpClient.PostAsync(PythonApiUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Python AI API returned error: {StatusCode}", response.StatusCode);
                return Result.Failure<RecommendationResponse>(
                    new Error("Recommendation.Failed", "فشل في الحصول على التوصيات"));
            }

            // قراءة الـ Response
            var pythonResponse = await response.Content.ReadFromJsonAsync<PythonApiResponse>();

            if (pythonResponse is null || pythonResponse.TopMatches is null)
                return Result.Failure<RecommendationResponse>(
                    new Error("Recommendation.Empty", "لا توجد وظائف مناسبة"));

            // تحويل الـ Response لـ format الـ ASP.NET
            var result = new RecommendationResponse(
                pythonResponse.TopMatches.Select(m => new JobMatchResult(
                    m.JobName,
                    m.JobLocation,
                    m.DistanceKm,
                    m.MatchScore
                )).ToList()
            );

            return Result.Success(result);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to connect to Python AI API");
            return Result.Failure<RecommendationResponse>(
                new Error("Recommendation.Unavailable", "خدمة التوصيات غير متاحة حالياً"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in RecommendationService");
            return Result.Failure<RecommendationResponse>(
                new Error("Recommendation.Error", "حدث خطأ غير متوقع"));
        }
    }

    // ===========================
    // Response Shape من Python API
    // ===========================
    private record PythonApiResponse(
        List<PythonJobMatch> TopMatches
    );

    private record PythonJobMatch(
        string JobName,
        int JobLocation,
        double DistanceKm,
        double MatchScore
    );
}
