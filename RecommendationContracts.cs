namespace SmartIndustrialRecruitment.Contracts.Recommendation;

// ===========================
// Request من الـ Frontend
// ===========================
public record RecommendationRequest(
    int WorkerJobType,    // نوع الوظيفة (0-15)
    int WorkerLocation,   // المحافظة (0-26)
    int WorkerExperience  // سنوات الخبرة (مثلاً 2, 3, 30)
);

// ===========================
// Response للـ Frontend
// ===========================
public record RecommendationResponse(
    List<JobMatchResult> TopMatches
);

public record JobMatchResult(
    string JobName,        // اسم الوظيفة بالعربي
    int JobLocation,       // رقم المحافظة
    double DistanceKm,     // المسافة بالكيلومتر
    double MatchScore      // نسبة التطابق (0 - 1)
);
