using System.Globalization;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// API endpoint to analyze students
app.MapGet("/api/analyze", () =>
{
    var students = LoadStudents();
    var analyses = students.Select(AnalyzeStudent).ToList();
    return Results.Json(analyses);
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
        if (parts.Length >= 13)
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
                Notes: parts[12]
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

    // === RISK LEVEL CLASSIFICATION ===
    // Risk levels now consider BOTH current state AND trends
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
        TrendAnalysis: trendAnalysis
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
    string Notes
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

// Analysis result with trend data
record StudentAnalysis(
    Student Student,
    string RiskLevel,
    int RiskScore,
    string RiskExplanation,
    List<string> Recommendations,
    TrendAnalysis TrendAnalysis
);
