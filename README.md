# 🎓 Path - AI-Powered Student Risk Monitoring System

![Risk Monitoring Dashboard](https://img.shields.io/badge/AI-Risk%20Detection-blue)
![Status](https://img.shields.io/badge/status-demo--ready-success)
![ASP.NET](https://img.shields.io/badge/ASP.NET%20Core-7.0-purple)

## 🚀 Quick Start

### Prerequisites
- Visual Studio 2022
- .NET 7.0 SDK

### Run the Application

```bash
cd C:\Users\yahli\Desktop\pathgit\Path
dotnet run
```

Then open your browser to: **http://localhost:5000**

---

## 🎯 What Does This System Do?

**Path** is an AI-powered educational platform that identifies at-risk students **BEFORE they fail**.

### Core Innovation
Most systems react to poor grades. **We detect risk weeks earlier** by analyzing:
- 📊 **Grade trends** (not just current grades)
- 📅 **Absence patterns** (early warning sign)
- 👨‍🏫 **Teacher observations** (AI-analyzed)

---

## ✨ Key Features

### 1. 🤖 AI Risk Analysis
- **Rule-based AI** that scores risk from 0-100
- Considers **current state + trends + teacher feedback**
- Three risk levels: 🔴 Red (high risk), 🟡 Yellow (early decline), 🟢 Green (stable)

### 2. 📈 Visual Trend Graphs
- **Grades line chart** - See improvement or decline over 4 quarters
- **Absences bar chart** - Identify concerning absence patterns
- Built with Chart.js for smooth, interactive visuals

### 3. 👨‍🏫 Teacher Feedback Integration
- Teachers write free-text comments about students
- **AI reads and analyzes** the comments using keyword detection
- Automatically adjusts risk score based on teacher insights
- Example: Teacher writes "stressed lately" → AI adds counselor referral recommendation

### 4. 🎯 Actionable Recommendations
- Not just "high risk" - system provides **specific next steps**:
  - "Schedule family meeting to discuss attendance"
  - "Refer to school counselor for emotional support"
  - "Create early intervention plan to reverse trend"

### 5. 📊 Smart Dashboard
- Color-coded risk levels
- Click any student for detailed analysis
- Statistics: Count of students in each risk category

---

## 📂 Project Structure

```
Path/
├── Program.cs                 # Backend API + AI logic
├── students.csv              # Student data (18 students with diverse risk profiles)
├── wwwroot/
│   ├── index.html            # Dashboard UI
│   ├── style.css             # Styling
│   ├── script.js             # Frontend logic + charts
│   └── pathLogo.jpeg         # Logo
├── AI_LOGIC_EXPLAINED.md     # Detailed AI logic documentation
└── README.md                 # This file
```

---

## 🧠 AI Logic Overview

### Risk Score Calculation (0-100 points)

#### Current State (0-50 points)
- Low grades: up to 30 points
- High absences: up to 15 points
- Behavioral issues: up to 5 points

#### Trend Analysis (0-50 points)
- Grade decline: up to 30 points
- Absence increase: up to 15 points
- Consecutive declines: bonus points

#### Teacher Comment AI (±20 points)
- Detects keywords like: "stressed", "family issues", "unmotivated"
- Adjusts risk score accordingly
- Adds contextual recommendations

**For detailed AI logic, see: [AI_LOGIC_EXPLAINED.md](AI_LOGIC_EXPLAINED.md)**

---

## 🎨 Demo Students (CSV Data)

The CSV includes 18 students across all risk levels:

### 🟢 Green (Stable) - 6 students
- נועה כהן - Perfect student
- תמר אברהם - Consistent and strong
- מיכל ברק - Outstanding performer
- גיא פרידמן - High and stable
- עמית בלום - Reliable achiever
- מאיה אדרי - Improving trend

### 🟡 Yellow (Early Warning) - 5 students
- **עידו לוי** - Grade 82 BUT absences rising (2→9 days) ⚠️
- **לירז שמש** - Grade 85 BUT high absences (9 days) ⚠️
- **יוסי טל** - Grade 74, 13 absences but manages self-study
- איתן בן שמחון - Gradual decline
- שני ורד - Consistent small decline

### 🔴 Red (High Risk) - 7 students
- שירה מזרחי - Deteriorating performance, family issues
- יונתן דוד - Critical decline, unfocused
- דניאל רוזן - Severe deterioration, concentration issues
- אורי גולן - Concerning trend
- רוני ישראלי - Urgent intervention needed, parents not cooperating
- רועי נחום - Critical case, rarely attends school

---

## 🎬 Demo Scenarios

### Scenario 1: Early Absence Detection
**Student: עידו לוי**
- Current grade: 82 (still good!)
- Absences: Increased from 2 → 9 days
- Teacher: "Looks tired lately - maybe working after school?"

**AI Result:** 🟡 YELLOW
- Detects rising absence pattern
- Reads teacher comment about work
- Recommendation: "Discuss work-school balance with family"

**Impact:** Caught early while grade is still good!

---

### Scenario 2: Teacher Insight Integration
**Student: יונתן דוד**
- Grade dropped: 65 → 45
- Absences: 8 → 15 days
- Teacher: "Not focused in class - seems very worried"

**AI Result:** 🔴 RED (Risk Score: 68)
- Low grade: +30 points
- High absences: +15 points
- Grade decline: +20 points
- Teacher keyword "worried": +10 points
- Recommendation: "🧠 Urgent: Refer to school counselor for emotional support"

---

## 🔧 API Endpoints

### GET `/api/analyze`
Returns analysis of all students with risk scores and recommendations.

**Response:**
```json
[
  {
    "student": {
      "id": 1,
      "name": "נועה כהן",
      "q1_AverageGrade": 94,
      "q4_AverageGrade": 92,
      "teacherComment": "מעולה בכל התחומים"
    },
    "riskLevel": "green",
    "riskScore": 5,
    "riskExplanation": "🟢 STABLE PERFORMANCE: Student shows consistent positive engagement.",
    "recommendations": ["⭐ Maintain current support strategies"],
    "trendAnalysis": {
      "gradeTrend": "Stable",
      "absenceTrend": "Stable",
      "quarterlyGrades": [94, 93, 92, 92],
      "quarterlyAbsences": [0, 1, 0, 1]
    },
    "teacherInsights": {
      "summary": "Positive teacher feedback",
      "riskAdjustment": -5,
      "recommendations": ["⭐ Continue current support"],
      "detectedConcerns": ["Positive teacher feedback"]
    }
  }
]
```

### GET `/api/student/{id}`
Returns detailed analysis for a single student.

### POST `/api/student/{id}/comment`
Updates teacher comment and re-analyzes risk.

**Request Body:**
```json
{
  "teacherComment": "נראה לחוץ לאחרונה - אולי בעיות בבית"
}
```

---

## 📊 Technologies Used

### Backend
- **ASP.NET Core 7.0** - Minimal API
- **C# Records** - Immutable data models
- **LINQ** - Data processing
- **Rule-based AI** - No ML libraries needed

### Frontend
- **HTML5/CSS3** - Responsive design
- **Vanilla JavaScript** - No frameworks needed
- **Chart.js** - Beautiful graphs
- **Fetch API** - Backend communication

### Data
- **CSV file** - Easy to edit and demo
- **Hebrew language** - Israeli student names and comments

---

## 🎓 Educational Value

This system demonstrates:
- ✅ **Proactive vs Reactive** monitoring
- ✅ **Multi-factor risk assessment** (not just grades)
- ✅ **AI augmentation** of human expertise (teacher observations)
- ✅ **Explainable AI** (every score is justified)
- ✅ **Actionable insights** (specific recommendations)

---

## 🔮 Future Enhancements

- [ ] Integration with school management systems
- [ ] Email/SMS alerts for high-risk students
- [ ] Automated intervention workflow
- [ ] Parent portal with progress updates
- [ ] Machine learning to improve prediction accuracy
- [ ] Multi-school comparison analytics

---

## 📝 How to Customize

### Add More Students
Edit `students.csv` - add rows with this format:
```
Id,Name,Q1_Grade,Q1_Abs,Q2_Grade,Q2_Abs,Q3_Grade,Q3_Abs,Q4_Grade,Q4_Abs,Late,Disruptions,Notes,TeacherComment
```

### Adjust Risk Thresholds
In `Program.cs`, modify the risk scoring in `AnalyzeStudent()` and `CalculateTrendAnalysis()`.

### Add New AI Keywords
In `Program.cs`, update `AnalyzeTeacherComment()` to detect new patterns in teacher comments.

---

## 🎯 Key Takeaways

1. **Early detection saves students** - Catch decline before failure
2. **Absences are early warning signs** - Often appear before grade drops
3. **Teacher observations matter** - AI can quantify qualitative insights
4. **Visual data is powerful** - Graphs reveal patterns instantly
5. **Explainability builds trust** - Show WHY a student is at risk

---

## 📞 Support

For questions or issues:
- Check [AI_LOGIC_EXPLAINED.md](AI_LOGIC_EXPLAINED.md) for detailed documentation
- Review code comments in `Program.cs`

---

## 📜 License

This is a demo/educational project built with AI assistance.

---

**🤖 Built with Claude Code - AI-Assisted Development**

*Making education better, one student at a time.* 🎓✨
