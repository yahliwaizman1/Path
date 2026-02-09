let studentsData = [];
let currentStudentId = null;
let gradesChart = null;
let absencesChart = null;

// Wait for DOM to be fully loaded
document.addEventListener('DOMContentLoaded', function() {
    console.log('DOM loaded, initializing...');

    // Page navigation
    const homePage = document.getElementById('homePage');
    const dashboardPage = document.getElementById('dashboardPage');
    const startAnalysisBtn = document.getElementById('startAnalysisBtn');
    const homeBtn = document.getElementById('homeBtn');

    // Debug: Check if elements exist
    console.log('Elements found:', {
        homePage: !!homePage,
        dashboardPage: !!dashboardPage,
        startAnalysisBtn: !!startAnalysisBtn,
        homeBtn: !!homeBtn
    });

    if (startAnalysisBtn) {
        startAnalysisBtn.addEventListener('click', () => {
            console.log('Start Analysis button clicked!');
            showDashboard();
            loadStudents();
        });
    } else {
        console.error('Start Analysis button not found!');
    }

    if (homeBtn) {
        homeBtn.addEventListener('click', () => {
            console.log('Home button clicked!');
            showHome();
        });
    }

    function showHome() {
        console.log('Showing home page');
        homePage.classList.add('active');
        dashboardPage.classList.remove('active');
    }

    function showDashboard() {
        console.log('Showing dashboard');
        homePage.classList.remove('active');
        dashboardPage.classList.add('active');
    }

    // Initialize save comment button
    const saveCommentBtn = document.getElementById('saveCommentBtn');
    if (saveCommentBtn) {
        saveCommentBtn.addEventListener('click', saveTeacherComment);
    }
});

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

    // Store current student ID for comment saving
    currentStudentId = student.id;

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

    // === CHARTS ===
    renderCharts(trend);

    // === TEACHER COMMENT & AI INSIGHTS ===
    displayTeacherComment(student, analysis.teacherInsights);

    detailPanel.classList.add('open');
}

// Render charts for student progress
function renderCharts(trendAnalysis) {
    const gradesCtx = document.getElementById('gradesChart').getContext('2d');
    const absencesCtx = document.getElementById('absencesChart').getContext('2d');

    // Destroy previous charts if they exist
    if (gradesChart) gradesChart.destroy();
    if (absencesChart) absencesChart.destroy();

    // Grades Chart
    gradesChart = new Chart(gradesCtx, {
        type: 'line',
        data: {
            labels: ['רבעון 1', 'רבעון 2', 'רבעון 3', 'רבעון 4'],
            datasets: [{
                label: 'ממוצע ציונים',
                data: trendAnalysis.quarterlyGrades,
                borderColor: '#667eea',
                backgroundColor: 'rgba(102, 126, 234, 0.1)',
                borderWidth: 3,
                pointBackgroundColor: '#667eea',
                pointBorderColor: '#fff',
                pointBorderWidth: 2,
                pointRadius: 6,
                pointHoverRadius: 8,
                tension: 0.3,
                fill: true
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: true,
            plugins: {
                title: {
                    display: true,
                    text: 'מגמת ציונים',
                    font: { size: 16, weight: 'bold' },
                    color: '#333'
                },
                legend: {
                    display: false
                }
            },
            scales: {
                y: {
                    beginAtZero: false,
                    min: 40,
                    max: 100,
                    ticks: {
                        font: { size: 12 }
                    }
                },
                x: {
                    ticks: {
                        font: { size: 12 }
                    }
                }
            }
        }
    });

    // Absences Chart
    absencesChart = new Chart(absencesCtx, {
        type: 'bar',
        data: {
            labels: ['רבעון 1', 'רבעון 2', 'רבעון 3', 'רבעון 4'],
            datasets: [{
                label: 'היעדרויות',
                data: trendAnalysis.quarterlyAbsences,
                backgroundColor: [
                    'rgba(102, 126, 234, 0.7)',
                    'rgba(102, 126, 234, 0.7)',
                    'rgba(102, 126, 234, 0.7)',
                    'rgba(239, 68, 68, 0.7)'
                ],
                borderColor: [
                    '#667eea',
                    '#667eea',
                    '#667eea',
                    '#ef4444'
                ],
                borderWidth: 2
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: true,
            plugins: {
                title: {
                    display: true,
                    text: 'מגמת היעדרויות',
                    font: { size: 16, weight: 'bold' },
                    color: '#333'
                },
                legend: {
                    display: false
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        stepSize: 2,
                        font: { size: 12 }
                    }
                },
                x: {
                    ticks: {
                        font: { size: 12 }
                    }
                }
            }
        }
    });
}

// Display teacher comment and AI insights
function displayTeacherComment(student, teacherInsights) {
    const teacherCommentArea = document.getElementById('teacherCommentArea');
    const aiInsights = document.getElementById('aiInsights');
    const aiSummary = document.getElementById('aiSummary');
    const aiConcerns = document.getElementById('aiConcerns');

    // Set current teacher comment
    teacherCommentArea.value = student.teacherComment || '';

    // Display AI insights if available
    if (teacherInsights && teacherInsights.detectedConcerns && teacherInsights.detectedConcerns.length > 0) {
        aiInsights.style.display = 'block';
        aiSummary.textContent = `סיכום: ${teacherInsights.summary}`;

        // Display concerns
        aiConcerns.innerHTML = '';
        teacherInsights.detectedConcerns.forEach(concern => {
            const concernDiv = document.createElement('div');
            concernDiv.className = 'concern-item';
            concernDiv.textContent = `⚠️ ${concern}`;
            aiConcerns.appendChild(concernDiv);
        });
    } else {
        aiInsights.style.display = 'none';
    }
}

// Save teacher comment function
async function saveTeacherComment() {
    const comment = document.getElementById('teacherCommentArea').value;
    const saveStatus = document.getElementById('saveStatus');
    const saveBtn = document.getElementById('saveCommentBtn');

    if (!currentStudentId) {
        console.error('No student selected');
        return;
    }

    saveBtn.disabled = true;
    saveBtn.textContent = '💾 שומר...';
    saveStatus.style.display = 'none';

    try {
        const response = await fetch(`/api/student/${currentStudentId}/comment`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                teacherComment: comment
            })
        });

        const result = await response.json();

        if (result.success) {
            saveStatus.className = 'save-status success';
            saveStatus.textContent = '✅ הערת המורה נשמרה בהצלחה! המערכת עדכנה את ניתוח הסיכון.';

            // Reload student data to reflect changes
            await loadStudents();

            // Find updated analysis
            const updatedAnalysis = studentsData.find(s => s.student.id === currentStudentId);
            if (updatedAnalysis) {
                // Update teacher insights display
                displayTeacherComment(updatedAnalysis.student, updatedAnalysis.teacherInsights);

                // Update recommendations and explanation
                const recList = document.getElementById('detailRecommendations');
                recList.innerHTML = '';
                updatedAnalysis.recommendations.forEach(rec => {
                    const li = document.createElement('li');
                    li.textContent = rec;
                    recList.appendChild(li);
                });

                document.getElementById('detailExplanation').textContent = updatedAnalysis.riskExplanation;

                const scoreEl = document.getElementById('detailRiskScore');
                scoreEl.textContent = updatedAnalysis.riskScore;
                scoreEl.className = `score-badge ${updatedAnalysis.riskLevel}`;
            }
        } else {
            throw new Error(result.error || 'Failed to save comment');
        }
    } catch (error) {
        console.error('Error saving comment:', error);
        saveStatus.className = 'save-status error';
        saveStatus.textContent = '❌ שגיאה בשמירת ההערה. אנא נסה שוב.';
    } finally {
        saveBtn.disabled = false;
        saveBtn.textContent = '💾 שמור הערה';
    }
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
