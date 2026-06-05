using SmartIndustrialRecruitment.Abstractions;
using SmartIndustrialRecruitment.Contracts.Recommendation;

namespace SmartIndustrialRecruitment.Services.Recommendation;

public interface IRecommendationService
{
    Task<Result<RecommendationResponse>> GetRecommendedJobsAsync(RecommendationRequest request);
}
