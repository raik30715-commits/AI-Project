from flask import Flask, request, jsonify
import pandas as pd
import joblib
from math import radians, cos, sin, sqrt, atan2

app = Flask(__name__)

# ===========================
# Load Model & Data ONCE
# ===========================
model = joblib.load("final_model_rf.pkl")
df = pd.read_csv("job_matching_dataset_realistic13.csv")

JOB_LIST = {
    0: "سباك", 1: "كهربائي", 2: "نجار", 3: "نقاش (دهان)",
    4: "عامل محارة", 5: "فني تكييف", 6: "فني صيانة أجهزة منزلية",
    7: "حداد", 8: "عامل نظافة", 9: "سائق خاص", 10: "عامل تحميل وتنزيل",
    11: "فني كاميرات مراقبة", 12: "فني دش", 13: "مبيض محارة",
    14: "فني سيراميك", 15: "وظيفة إضافية"
}

GOV_COORDS = {
    0: (30.06, 31.25), 1: (31.20, 29.92), 2: (31.17, 31.49), 3: (31.36, 31.67),
    4: (29.31, 30.84), 5: (30.88, 31.03), 6: (28.77, 29.23), 7: (30.58, 32.27),
    8: (31.31, 30.80), 9: (25.39, 32.49), 10: (29.57, 26.42), 11: (28.28, 30.53),
    12: (30.60, 30.99), 13: (24.55, 27.17), 14: (30.28, 33.62), 15: (31.08, 32.27),
    16: (30.33, 31.22), 17: (26.23, 32.99), 18: (24.68, 34.15), 19: (30.67, 31.16),
    20: (26.69, 32.17), 21: (29.31, 34.15), 22: (29.37, 32.17), 23: (30.98, 30.20),
    24: (27.18, 31.18), 25: (29.31, 30.84), 26: (28.82, 30.90)
}

def haversine(coord1, coord2):
    R = 6371
    lat1, lon1 = coord1
    lat2, lon2 = coord2
    phi1, phi2 = radians(lat1), radians(lat2)
    dphi = radians(lat2 - lat1)
    dlambda = radians(lon2 - lon1)
    a = sin(dphi/2)**2 + cos(phi1)*cos(phi2)*sin(dlambda/2)**2
    return R * 2 * atan2(sqrt(a), sqrt(1 - a))


# ===========================
# Recommendation Endpoint
# ===========================
@app.route("/recommend", methods=["POST"])
def recommend():
    data = request.get_json()

    # استقبال البيانات من ASP.NET
    worker_job  = data.get("worker_job_type")
    worker_gov  = data.get("worker_location")
    worker_exp  = data.get("worker_experience")

    # التحقق من البيانات
    if worker_job is None or worker_gov is None or worker_exp is None:
        return jsonify({"error": "missing fields"}), 400

    if worker_job not in range(16) or worker_gov not in range(27):
        return jsonify({"error": "invalid job type or location"}), 400

    worker_coord = GOV_COORDS[worker_gov]
    jobs = df.copy()
    scores = []

    for _, row in jobs.iterrows():
        job_type  = row["job_job_type"]
        job_gov   = row["job_location"]
        job_coord = GOV_COORDS.get(job_gov, worker_coord)

        distance_km   = haversine(worker_coord, job_coord)
        job_type_match = 1 if worker_job == job_type else 0

        features = pd.DataFrame([{
            "worker_job_type":  worker_job,
            "job_job_type":     job_type,
            "worker_location":  worker_gov,
            "job_location":     job_gov,
            "distance_km":      distance_km,
            "skill_overlap":    row["skill_overlap"],
            "skill_match_ratio":row["skill_match_ratio"],
            "experience_fit":   row["experience_fit"],
            "location_score":   row["location_score"],
            "job_type_match":   job_type_match
        }])

        prob = model.predict_proba(features)[0][1]
        scores.append(prob)

    jobs["match_score"] = scores
    best_jobs = jobs.sort_values(by="match_score", ascending=False).head(5)

    # تجهيز الـ Response
    result = []
    for _, row in best_jobs.iterrows():
        result.append({
            "jobName":    JOB_LIST.get(row["job_job_type"], "Unknown"),
            "jobLocation": int(row["job_location"]),
            "distanceKm": round(float(row.get("distance_km", 0)), 1),
            "matchScore": round(float(row["match_score"]), 3)
        })

    return jsonify({"topMatches": result}), 200


if __name__ == "__main__":
    app.run(host="0.0.0.0", port=5000, debug=False)
