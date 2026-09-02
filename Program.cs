using System.Globalization;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Configure to use port 5000 (HTTP only) to avoid port conflicts
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
builder.WebHost.UseUrls($"http://*:{port}");

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();
app.UseCors();

app.UseDefaultFiles();
app.UseStaticFiles();

// API endpoint to analyze all students
app.MapGet("/api/analyze", () =>
{
    var students = LoadStudents();
    var analyses = students.Select(AnalyzeStudent).ToList();
    return Results.Json(analyses, new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    });
});

// API endpoint to get single student analysis
app.MapGet("/api/student/{id}", (int id) =>
{
    var students = LoadStudents();
    var student = students.FirstOrDefault(s => s.Id == id);

    if (student == null)
        return Results.NotFound(new { error = "Student not found" });

    var analysis = AnalyzeStudent(student);
    return Results.Json(analysis, new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    });
});

// API endpoint to update teacher comment
app.MapPost("/api/student/{id}/comment", async (int id, HttpRequest request) =>
{
    using var reader = new StreamReader(request.Body);
    var body = await reader.ReadToEndAsync();
    var data = JsonSerializer.Deserialize<Dictionary<string, string>>(body);

    if (data == null || !data.ContainsKey("teacherComment"))
        return Results.BadRequest(new { error = "teacherComment field required" });

    var newComment = data["teacherComment"];

    // Update CSV file
    var lines = File.ReadAllLines("students.csv").ToList();
    bool updated = false;

    for (int i = 1; i < lines.Count; i++) // Skip header
    {
        var parts = lines[i].Split(',');
        if (parts.Length >= 14 && int.Parse(parts[0]) == id)
        {
            // Update the TeacherComment field (index 13)
            parts[13] = newComment;
            lines[i] = string.Join(',', parts);
            updated = true;
            break;
        }
    }

    if (!updated)
        return Results.NotFound(new { error = "Student not found" });

    File.WriteAllLines("students.csv", lines);

    // Return updated analysis
    var students = LoadStudents();
    var student = students.FirstOrDefault(s => s.Id == id);
    var analysis = AnalyzeStudent(student!);

    return Results.Json(new {
        success = true,
        message = "Teacher comment updated successfully",
        analysis
    }, new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    });
});

app.Run();

// Load students from CSV
static List<Student> LoadStudents()
{
    var students = new List<Student>();
    var lines = File.ReadAllLines("students.csv");

    for (int i = 1; i < lines.Length; i++) // Skip header
    {
        var parts = lines[i].Split(',');
        if (parts.Length >= 14)
        {
            students.Add(new Student(
                Id: int.Parse(parts[0]),
                Name: parts[1],
                Q1_AverageGrade: double.Parse(parts[2], CultureInfo.InvariantCulture),
                Q1_Absences: int.Parse(parts[3]),
                Q2_AverageGrade: double.Parse(parts[4], CultureInfo.InvariantCulture),
                Q2_Absences: int.Parse(parts[5]),
                Q3_AverageGrade: double.Parse(parts[6], CultureInfo.InvariantCulture),
                Q3_Absences: int.Parse(parts[7]),
                Q4_AverageGrade: double.Parse(parts[8], CultureInfo.InvariantCulture),
                Q4_Absences: int.Parse(parts[9]),
                LateArrivals: int.Parse(parts[10]),
                Disruptions: int.Parse(parts[11]),
                Notes: parts[12],
                TeacherComment: parts.Length > 13 ? parts[13] : ""
            ));
        }
    }

    return students;
}

// AI Risk Analysis Engine with Trend Detection
static StudentAnalysis AnalyzeStudent(Student student)
{
    // Calculate current quarter (Q4) metrics
    double currentGrade = student.Q4_AverageGrade;
    int currentAbsences = student.Q4_Absences;

    // TREND ANALYSIS: Detect patterns over time
    var trendAnalysis = CalculateTrendAnalysis(student);

    // TEACHER COMMENT ANALYSIS: Extract insights from teacher observations
    var teacherInsights = AnalyzeTeacherComment(student.TeacherComment);

    // Base risk score (0-100)
    int riskScore = 0;
    var factors = new List<string>();
    var recommendations = new List<string>();

    // === CURRENT STATE ANALYSIS (0-50 points) ===

    // Factor 1: Current grade (0-30 points)
    if (currentGrade < 60)
    {
        riskScore += 30;
        factors.Add($"Low current grade ({currentGrade:F1})");
    }
    else if (currentGrade < 75)
    {
        riskScore += 15;
        factors.Add($"Below-average grade ({currentGrade:F1})");
    }

    // Factor 2: Current absences (0-15 points)
    if (currentAbsences > 10)
    {
        riskScore += 15;
        factors.Add($"High absences ({currentAbsences} days in Q4)");
    }
    else if (currentAbsences > 5)
    {
        riskScore += 8;
        factors.Add($"Moderate absences ({currentAbsences} days in Q4)");
    }

    // Factor 3: Behavioral issues (0-5 points)
    if (student.Disruptions > 5)
    {
        riskScore += 5;
        factors.Add($"Behavioral concerns ({student.Disruptions} incidents)");
    }

    // === TREND-BASED ANALYSIS (0-50 points) ===
    // This is the KEY ENHANCEMENT: detect deterioration even if current state seems okay

    int trendScore = trendAnalysis.TrendRiskScore;
    riskScore += trendScore;

    if (trendAnalysis.GradeTrend == "Declining")
    {
        factors.Add($"Grade decline detected ({trendAnalysis.GradeDrop:F1} point drop since Q1)");

        if (trendAnalysis.ConsecutiveDeclines >= 2)
        {
            recommendations.Add($"⚠️ Grades declined for {trendAnalysis.ConsecutiveDeclines} consecutive quarters");
            recommendations.Add("📚 Immediate academic intervention recommended");
        }
        else
        {
            recommendations.Add("📉 Monitor grade trend closely");
        }
    }

    if (trendAnalysis.AbsenceTrend == "Increasing")
    {
        factors.Add($"Absence growth detected (+{trendAnalysis.AbsenceIncrease} days since Q1)");

        if (trendAnalysis.AbsenceIncrease >= 5)
        {
            recommendations.Add($"🏠 Absences increased by {trendAnalysis.AbsenceIncrease} days since Q1 - schedule parent meeting");
        }
        else
        {
            recommendations.Add("📞 Contact family to discuss attendance pattern");
        }
    }

    if (trendAnalysis.OverallTrend == "Deteriorating")
    {
        recommendations.Add("🆘 Preventive action needed before next quarter");
        recommendations.Add("📋 Create early intervention plan to reverse trend");
    }

    // === BEHAVIORAL RECOMMENDATIONS ===
    if (student.LateArrivals > 8)
    {
        recommendations.Add($"⏰ Address chronic lateness ({student.LateArrivals} late arrivals)");
    }

    if (student.Disruptions > 5)
    {
        recommendations.Add("🧠 Refer to school counselor for behavioral support");
    }

    // === TEACHER COMMENT ANALYSIS ===
    // AI reads teacher observations and adjusts risk assessment accordingly
    if (teacherInsights.RiskAdjustment != 0)
    {
        riskScore += teacherInsights.RiskAdjustment;
        factors.Add($"Teacher observation: {teacherInsights.Summary}");
    }

    // Add teacher-informed recommendations
    if (teacherInsights.Recommendations.Any())
    {
        recommendations.AddRange(teacherInsights.Recommendations);
    }

    // === RISK LEVEL CLASSIFICATION ===
    // Risk levels now consider BOTH current state AND trends AND teacher observations
    string riskLevel;
    string explanation;

    if (riskScore >= 50)
    {
        // RED: High risk due to poor current state OR severe negative trends
        riskLevel = "red";
        explanation = "🔴 HIGH RISK: " + string.Join(". ", factors) + ".";

        if (!recommendations.Any())
        {
            recommendations.Add("👀 Monitor student progress weekly");
        }
        recommendations.Add("🚨 Immediate comprehensive intervention required");
    }
    else if (riskScore >= 25)
    {
        // YELLOW: Early warning - may still have decent grades but showing decline
        riskLevel = "yellow";
        explanation = "🟡 EARLY DECLINE DETECTED: " + string.Join(". ", factors) + ".";

        if (!recommendations.Any())
        {
            recommendations.Add("👀 Monitor student progress bi-weekly");
        }
        recommendations.Add("💪 Provide support to reverse negative trend");
    }
    else
    {
        // GREEN: Stable or improving
        riskLevel = "green";
        explanation = "🟢 STABLE PERFORMANCE: Student shows consistent positive engagement.";

        if (trendAnalysis.OverallTrend == "Improving")
        {
            explanation += " Performance improving over time.";
            recommendations.Add("⭐ Continue current support - positive trend observed");
        }
        else
        {
            recommendations.Add("⭐ Maintain current support strategies");
        }

        recommendations.Add("🎯 Consider leadership or mentorship opportunities");
    }

    return new StudentAnalysis(
        Student: student,
        RiskLevel: riskLevel,
        RiskScore: riskScore,
        RiskExplanation: explanation,
        Recommendations: recommendations,
        TrendAnalysis: trendAnalysis,
        TeacherInsights: teacherInsights
    );
}

// TREND CALCULATION ENGINE
// This is where the "smart" AI logic happens
static TrendAnalysis CalculateTrendAnalysis(Student student)
{
    // Get all quarterly grades and absences
    var grades = new[] { student.Q1_AverageGrade, student.Q2_AverageGrade, student.Q3_AverageGrade, student.Q4_AverageGrade };
    var absences = new[] { student.Q1_Absences, student.Q2_Absences, student.Q3_Absences, student.Q4_Absences };

    // === GRADE TREND ANALYSIS ===
    double gradeDrop = student.Q1_AverageGrade - student.Q4_AverageGrade;
    int consecutiveDeclines = 0;

    // Count consecutive quarters with declining grades
    for (int i = 0; i < grades.Length - 1; i++)
    {
        if (grades[i + 1] < grades[i])
        {
            consecutiveDeclines++;
        }
    }

    string gradeTrend;
    if (gradeDrop > 10) gradeTrend = "Declining";
    else if (gradeDrop < -5) gradeTrend = "Improving";
    else gradeTrend = "Stable";

    // === ABSENCE TREND ANALYSIS ===
    int absenceIncrease = student.Q4_Absences - student.Q1_Absences;

    string absenceTrend;
    if (absenceIncrease > 3) absenceTrend = "Increasing";
    else if (absenceIncrease < -2) absenceTrend = "Decreasing";
    else absenceTrend = "Stable";

    // === TREND RISK SCORE (0-50 points) ===
    int trendRiskScore = 0;

    // Grade decline risk (0-30 points)
    if (gradeDrop > 20)
    {
        trendRiskScore += 30; // Severe decline
    }
    else if (gradeDrop > 10)
    {
        trendRiskScore += 20; // Significant decline
    }
    else if (gradeDrop > 5)
    {
        trendRiskScore += 10; // Moderate decline
    }

    // Consecutive decline bonus (high risk indicator)
    if (consecutiveDeclines >= 3)
    {
        trendRiskScore += 10; // All quarters declining = critical
    }
    else if (consecutiveDeclines >= 2)
    {
        trendRiskScore += 5; // Two quarters declining = concerning
    }

    // Absence growth risk (0-15 points)
    if (absenceIncrease > 8)
    {
        trendRiskScore += 15;
    }
    else if (absenceIncrease > 5)
    {
        trendRiskScore += 10;
    }
    else if (absenceIncrease > 3)
    {
        trendRiskScore += 5;
    }

    // === OVERALL TREND ===
    string overallTrend;
    if (gradeTrend == "Declining" || absenceTrend == "Increasing")
    {
        overallTrend = "Deteriorating";
    }
    else if (gradeTrend == "Improving" && absenceTrend != "Increasing")
    {
        overallTrend = "Improving";
    }
    else
    {
        overallTrend = "Stable";
    }

    return new TrendAnalysis(
        GradeTrend: gradeTrend,
        AbsenceTrend: absenceTrend,
        OverallTrend: overallTrend,
        GradeDrop: gradeDrop,
        AbsenceIncrease: absenceIncrease,
        ConsecutiveDeclines: consecutiveDeclines,
        TrendRiskScore: trendRiskScore,
        QuarterlyGrades: grades.ToList(),
        QuarterlyAbsences: absences.ToList()
    );
}

// TEACHER COMMENT ANALYSIS ENGINE
// This is where AI "reads" teacher observations and extracts actionable insights
static TeacherInsights AnalyzeTeacherComment(string comment)
{
    if (string.IsNullOrWhiteSpace(comment))
    {
        return new TeacherInsights(
            Summary: "No teacher observation recorded",
            RiskAdjustment: 0,
            Recommendations: new List<string>(),
            DetectedConcerns: new List<string>()
        );
    }

    var lowerComment = comment.ToLower();
    var concerns = new List<string>();
    var recommendations = new List<string>();
    int riskAdjustment = 0;

    // === AI KEYWORD DETECTION: Identify concerning patterns ===

    // Psychological/emotional concerns
    if (ContainsAny(lowerComment, new[] { "מוטרד", "לחץ", "דכאון", "חרדה", "stress", "worried", "anxious" }))
    {
        concerns.Add("Emotional/psychological distress");
        riskAdjustment += 10;
        recommendations.Add("🧠 Urgent: Refer to school counselor for emotional support");
    }

    // Family problems
    if (ContainsAny(lowerComment, new[] { "משפחה", "הורים", "בית", "family", "home", "parents" }))
    {
        concerns.Add("Family/home situation");
        riskAdjustment += 8;
        recommendations.Add("👨‍👩‍👧 Schedule family meeting to understand home context");
    }

    // Concentration/focus issues
    if (ContainsAny(lowerComment, new[] { "ריכוז", "ממוקד", "קשב", "אבחון", "concentration", "focus", "attention", "adhd" }))
    {
        concerns.Add("Concentration difficulties");
        riskAdjustment += 5;
        recommendations.Add("🔍 Consider professional assessment for learning challenges");
    }

    // Motivation issues
    if (ContainsAny(lowerComment, new[] { "מוטיבציה", "חסר", "motivation", "unmotivated", "disengaged" }))
    {
        concerns.Add("Lack of motivation");
        riskAdjustment += 5;
        recommendations.Add("💬 Personal conversation needed to understand underlying causes");
    }

    // Health/medical issues
    if (ContainsAny(lowerComment, new[] { "חולה", "מחלות", "עייף", "sick", "illness", "tired", "health" }))
    {
        concerns.Add("Health-related absences");
        riskAdjustment += 3;
        recommendations.Add("🏥 Verify medical situation with parents - may need accommodations");
    }

    // Work/employment concerns
    if (ContainsAny(lowerComment, new[] { "עבודה", "עובד", "work", "job", "employment" }))
    {
        concerns.Add("Outside employment affecting studies");
        riskAdjustment += 7;
        recommendations.Add("⚖️ Discuss work-school balance with student and family");
    }

    // Parent cooperation issues
    if (ContainsAny(lowerComment, new[] { "לא משתפים", "לא מגיבים", "not cooperating", "unresponsive" }))
    {
        concerns.Add("Lack of parental cooperation");
        riskAdjustment += 10;
        recommendations.Add("⚠️ Escalate to school administration - parental involvement critical");
    }

    // Critical/severe situation
    if (ContainsAny(lowerComment, new[] { "חמור", "קריטי", "דחוף", "רשויות", "critical", "severe", "urgent", "authorities" }))
    {
        concerns.Add("Critical situation requiring immediate action");
        riskAdjustment += 15;
        recommendations.Add("🚨 URGENT: Immediate intervention required - involve school administration");
    }

    // === POSITIVE INDICATORS: Reduce risk if teacher sees improvement ===

    if (ContainsAny(lowerComment, new[] { "השתפר", "טוב", "מצוין", "מוצלח", "improved", "excellent", "great", "successful" }))
    {
        concerns.Add("Positive teacher feedback - lower concern");
        riskAdjustment -= 5;
        recommendations.Add("⭐ Continue current support - positive teacher observation");
    }

    // Learning difficulties but stable
    if (ContainsAny(lowerComment, new[] { "לומד לבד", "מצליח למרות", "manages despite", "self-study" }))
    {
        concerns.Add("Resilient student - manages independently");
        riskAdjustment -= 3;
        recommendations.Add("💪 Student shows resilience - consider gifted/independent learning track");
    }

    // Summary generation
    string summary = concerns.Any()
        ? string.Join(", ", concerns.Take(2))
        : "General teacher observation";

    return new TeacherInsights(
        Summary: summary,
        RiskAdjustment: Math.Clamp(riskAdjustment, -10, 20), // Cap adjustment to reasonable range
        Recommendations: recommendations,
        DetectedConcerns: concerns
    );
}

// Helper function for keyword matching
static bool ContainsAny(string text, string[] keywords)
{
    return keywords.Any(keyword => text.Contains(keyword, StringComparison.OrdinalIgnoreCase));
}

// === DATA MODELS ===

// Student model with quarterly data
record Student(
    int Id,
    string Name,
    double Q1_AverageGrade,
    int Q1_Absences,
    double Q2_AverageGrade,
    int Q2_Absences,
    double Q3_AverageGrade,
    int Q3_Absences,
    double Q4_AverageGrade,
    int Q4_Absences,
    int LateArrivals,
    int Disruptions,
    string Notes,
    string TeacherComment
);

// Trend analysis result
record TrendAnalysis(
    string GradeTrend,           // "Declining", "Stable", "Improving"
    string AbsenceTrend,         // "Increasing", "Stable", "Decreasing"
    string OverallTrend,         // "Deteriorating", "Stable", "Improving"
    double GradeDrop,            // Q1 to Q4 grade change
    int AbsenceIncrease,         // Q1 to Q4 absence change
    int ConsecutiveDeclines,     // Number of consecutive quarters with declining grades
    int TrendRiskScore,          // Risk score based on trends (0-50)
    List<double> QuarterlyGrades,
    List<int> QuarterlyAbsences
);

// Teacher comment insights (AI analysis of teacher observations)
record TeacherInsights(
    string Summary,                  // Brief summary of teacher's concern
    int RiskAdjustment,              // Risk score adjustment (-10 to +20)
    List<string> Recommendations,    // Recommendations based on teacher input
    List<string> DetectedConcerns    // Specific concerns identified by AI
);

// Analysis result with trend data and teacher insights
record StudentAnalysis(
    Student Student,
    string RiskLevel,
    int RiskScore,
    string RiskExplanation,
    List<string> Recommendations,
    TrendAnalysis TrendAnalysis,
    TeacherInsights TeacherInsights
);
