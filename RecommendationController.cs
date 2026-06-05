using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartIndustrialRecruitment.Contracts.Recommendation;
using SmartIndustrialRecruitment.Services.Recommendation;

namespace SmartIndustrialRecruitment.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // يتطلب JWT Token
public class RecommendationController : ControllerBase
{
    private readonly IRecommendationService _recommendationService;

    public RecommendationController(IRecommendationService recommendationService)
    {
        _recommendationService = recommendationService;
    }

    /// <summary>
    /// يرسل بيانات العامل ويرجع أحسن 5 وظائف مناسبة
    /// POST /api/recommendation/jobs
    /// </summary>
    [HttpPost("jobs")]
    public async Task<IActionResult> GetRecommendedJobs([FromBody] RecommendationRequest request)
    {
        var result = await _recommendationService.GetRecommendedJobsAsync(request);

        if (!result.IsSuccess)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }
}
