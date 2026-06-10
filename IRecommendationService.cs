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
private readonly string _pythonApiUrl;

public RecommendationService(
    HttpClient httpClient,
    ILogger<RecommendationService> logger,
    IConfiguration configuration)
{
    _httpClient = httpClient;
    _logger = logger;

    var baseUrl = configuration["PythonAI:BaseUrl"];

    if (string.IsNullOrWhiteSpace(baseUrl))
        throw new InvalidOperationException("PythonAI:BaseUrl is not configured.");

    _pythonApiUrl = $"{baseUrl.TrimEnd('/')}/recommend";
}

public async Task<Result<RecommendationResponse>> GetRecommendedJobsAsync(RecommendationRequest request)
{
    try
    {
        var pythonRequest = new
        {
            worker_job_type = request.WorkerJobType,
            worker_location = request.WorkerLocation,
            worker_experience = request.WorkerExperience
        };

        var json = JsonSerializer.Serialize(pythonRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(_pythonApiUrl, content);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError(
                "Python AI API returned error: {StatusCode}",
                response.StatusCode);

            return Result.Failure<RecommendationResponse>(
                new Error(
                    "Recommendation.Failed",
                    $"Python AI API returned {response.StatusCode}"
                ));
        }

        var pythonResponse =
            await response.Content.ReadFromJsonAsync<PythonApiResponse>();

        if (pythonResponse is null || pythonResponse.TopMatches is null)
        {
            return Result.Failure<RecommendationResponse>(
                new Error(
                    "Recommendation.Empty",
                    "No recommendations returned"
                ));
        }

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
            new Error(
                "Recommendation.Unavailable",
                "Recommendation service is unavailable"
            ));
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Unexpected error in RecommendationService");

        return Result.Failure<RecommendationResponse>(
            new Error(
                "Recommendation.Error",
                ex.Message
            ));
    }
}

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
