// ===========================
// أضف دا في Dependancies.cs
// ===========================

// في نفس نمط باقي الـ services الموجودة في المشروع:
builder.Services.AddHttpClient<IRecommendationService, RecommendationService>();
builder.Services.AddScoped<IRecommendationService, RecommendationService>();


// ===========================
// appsettings.json - أضف الـ URL
// ===========================
/*
{
  "PythonAI": {
    "BaseUrl": "http://localhost:5000"
  }
}
*/
