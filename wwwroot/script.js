let studentsData = [];

// Page navigation
const homePage = document.getElementById('homePage');
const dashboardPage = document.getElementById('dashboardPage');
const startAnalysisBtn = document.getElementById('startAnalysisBtn');
const homeBtn = document.getElementById('homeBtn');

startAnalysisBtn.addEventListener('click', () => {
    showDashboard();
    loadStudents();
});

homeBtn.addEventListener('click', () => {
    showHome();
});

function showHome() {
    homePage.classList.add('active');
    dashboardPage.classList.remove('active');
}

function showDashboard() {
    homePage.classList.remove('active');
    dashboardPage.classList.add('active');
}

// Load and display students
async function loadStudents() {
    const loadingIndicator = document.getElementById('loadingIndicator');
    const studentTable = document.getElementById('studentTable');

    loadingIndicator.style.display = 'block';
    studentTable.style.display = 'none';

    try {
        const response = await fetch('/api/analyze');
        studentsData = await response.json();

        setTimeout(() => {
            displayStudents(studentsData);
            updateStats(studentsData);
            loadingIndicator.style.display = 'none';
            studentTable.style.display = 'table';
        }, 1500); // Simulate AI processing time

    } catch (error) {
        console.error('Error loading students:', error);
        loadingIndicator.innerHTML = '<p style="color: #ef4444;">שגיאה בטעינת הנתונים. אנא נסה שוב.</p>';
    }
}

function displayStudents(students) {
    const tbody = document.getElementById('studentTableBody');
    tbody.innerHTML = '';

    students.forEach(analysis => {
        const student = analysis.student;
        const row = document.createElement('tr');
        row.className = analysis.riskLevel;

        // Use Q4 (current) data for table display
        row.innerHTML = `
            <td><strong>${student.name}</strong></td>
            <td>${student.q4_AverageGrade.toFixed(1)}</td>
            <td>${student.q4_Absences}</td>
            <td>${student.lateArrivals}</td>
            <td>${student.disruptions}</td>
            <td><span class="risk-badge ${analysis.riskLevel}">${analysis.riskLevel.toUpperCase()}</span></td>
        `;

        row.addEventListener('click', () => showStudentDetail(analysis));
        tbody.appendChild(row);
    });
}

function updateStats(students) {
    const greenCount = students.filter(s => s.riskLevel === 'green').length;
    const yellowCount = students.filter(s => s.riskLevel === 'yellow').length;
    const redCount = students.filter(s => s.riskLevel === 'red').length;

    document.getElementById('greenCount').textContent = greenCount;
    document.getElementById('yellowCount').textContent = yellowCount;
    document.getElementById('redCount').textContent = redCount;
}

// Detail panel
const detailPanel = document.getElementById('detailPanel');
const closePanelBtn = document.getElementById('closePanelBtn');

closePanelBtn.addEventListener('click', () => {
    detailPanel.classList.remove('open');
});

function showStudentDetail(analysis) {
    const student = analysis.student;
    const trend = analysis.trendAnalysis;

    // Student name
    document.getElementById('detailName').textContent = student.name;

    // === TREND OVERVIEW ===
    const overallTrendEl = document.getElementById('detailOverallTrend');
    overallTrendEl.textContent = getTrendDisplay(trend.overallTrend);
    overallTrendEl.className = 'trend-value ' + getTrendClass(trend.overallTrend);

    const gradeTrendEl = document.getElementById('detailGradeTrend');
    gradeTrendEl.textContent = getTrendDisplay(trend.gradeTrend);
    gradeTrendEl.className = 'trend-value ' + getTrendClass(trend.gradeTrend);

    const absenceTrendEl = document.getElementById('detailAbsenceTrend');
    absenceTrendEl.textContent = getTrendDisplay(trend.absenceTrend);
    absenceTrendEl.className = 'trend-value ' + getTrendClass(trend.absenceTrend);

    // === QUARTERLY DATA ===
    document.getElementById('q1Grade').textContent = student.q1_AverageGrade.toFixed(1);
    document.getElementById('q1Absences').textContent = student.q1_Absences;

    document.getElementById('q2Grade').textContent = student.q2_AverageGrade.toFixed(1);
    document.getElementById('q2Absences').textContent = student.q2_Absences;

    document.getElementById('q3Grade').textContent = student.q3_AverageGrade.toFixed(1);
    document.getElementById('q3Absences').textContent = student.q3_Absences;

    document.getElementById('q4Grade').textContent = student.q4_AverageGrade.toFixed(1);
    document.getElementById('q4Absences').textContent = student.q4_Absences;

    // === CURRENT METRICS ===
    document.getElementById('detailGrade').textContent = student.q4_AverageGrade.toFixed(1);
    document.getElementById('detailAbsences').textContent = student.q4_Absences;
    document.getElementById('detailLate').textContent = student.lateArrivals;
    document.getElementById('detailDisruptions').textContent = student.disruptions;

    // === RISK SCORE ===
    const scoreEl = document.getElementById('detailRiskScore');
    scoreEl.textContent = analysis.riskScore;
    scoreEl.className = `score-badge ${analysis.riskLevel}`;

    // === EXPLANATION ===
    document.getElementById('detailExplanation').textContent = analysis.riskExplanation;

    // === RECOMMENDATIONS ===
    const recList = document.getElementById('detailRecommendations');
    recList.innerHTML = '';
    analysis.recommendations.forEach(rec => {
        const li = document.createElement('li');
        li.textContent = rec;
        recList.appendChild(li);
    });

    // === NOTES ===
    document.getElementById('detailNotes').textContent = student.notes || 'No notes available.';

    detailPanel.classList.add('open');
}

// Helper function to display trend with icons
function getTrendDisplay(trend) {
    const trendTranslations = {
        'Declining': 'יורדת',
        'Deteriorating': 'מדרדרת',
        'Increasing': 'עולה',
        'Improving': 'משתפרת',
        'Decreasing': 'יורדת',
        'Stable': 'יציבה'
    };

    const hebrewTrend = trendTranslations[trend] || trend;

    if (trend === 'Declining' || trend === 'Deteriorating' || trend === 'Increasing') {
        return `${hebrewTrend} ↓`;
    } else if (trend === 'Improving' || trend === 'Decreasing') {
        return `${hebrewTrend} ↑`;
    } else {
        return `${hebrewTrend} →`;
    }
}

// Helper function to get CSS class for trend
function getTrendClass(trend) {
    if (trend === 'Declining' || trend === 'Deteriorating' || trend === 'Increasing') {
        return 'trend-negative';
    } else if (trend === 'Improving' || trend === 'Decreasing') {
        return 'trend-positive';
    } else {
        return 'trend-stable';
    }
}
