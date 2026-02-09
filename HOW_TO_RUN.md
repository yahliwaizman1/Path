# 🚀 How to Run Path - Student Risk Monitoring System

## ✅ Problem Fixed!

The port conflict issue has been resolved. The application now runs on a single, consistent port: **5000**

---

## 🎯 Method 1: Double-Click START.bat (EASIEST!)

1. Navigate to: `C:\Users\yahli\Desktop\pathgit\Path`
2. **Double-click:** `START.bat`
3. The application will:
   - Stop any existing instances
   - Start the server
   - Open your browser automatically to http://localhost:5000
4. **Done!** The dashboard should load.

---

## 🎯 Method 2: Visual Studio 2022

1. **Open Visual Studio 2022**
2. **Open:** `C:\Users\yahli\Desktop\pathgit\Path\Path.csproj`
3. **Press F5** (or click the green ▶ Run button)
4. Browser opens automatically to http://localhost:5000
5. **Click:** "🚀 התחל ניתוח AI"

---

## 🎯 Method 3: Command Line

Open Command Prompt or PowerShell:

```bash
cd C:\Users\yahli\Desktop\pathgit\Path
dotnet run
```

Then open browser to: **http://localhost:5000**

---

## 🌐 Access the Application

Once running, the application is available at:

### **http://localhost:5000**

**Note:** Always use port 5000 now (no more random ports!)

---

## 🛑 How to Stop

### If you see "port already in use" error:

**Option A - Use the batch file:**
```bash
taskkill /F /IM dotnet.exe
taskkill /F /IM Path.exe
```

**Option B - Quick command:**
```bash
cd C:\Users\yahli\Desktop\pathgit\Path
taskkill /F /IM dotnet.exe && dotnet run
```

### If running in Visual Studio:
- Press **Shift+F5** or click the red ■ Stop button

### If running in Command Prompt:
- Press **Ctrl+C**

---

## 🔧 What Was Fixed

### Problem:
```
System.IO.IOException: Failed to bind to address https://127.0.0.1:62873:
address already in use
```

### Solution:
1. ✅ Updated `Program.cs` to use port 5000 only (no HTTPS conflicts)
2. ✅ Updated `launchSettings.json` to match
3. ✅ Created `START.bat` for easy launching
4. ✅ Killed all background processes

### Changes Made:
**Program.cs (Line 6-7):**
```csharp
// Configure to use port 5000 (HTTP only) to avoid port conflicts
builder.WebHost.UseUrls("http://localhost:5000");
```

**launchSettings.json (Line 9):**
```json
"applicationUrl": "http://localhost:5000"
```

---

## 📝 Quick Troubleshooting

| Problem | Solution |
|---------|----------|
| "Port already in use" | Run: `taskkill /F /IM dotnet.exe` then try again |
| Browser shows nothing | Make sure you're using http://localhost:5000 |
| Button doesn't work | Check browser console (F12) for errors |
| Can't find dotnet | Install .NET 7.0 SDK from Microsoft |

---

## ✨ Using the Application

1. **Landing Page** - Click "🚀 התחל ניתוח AI"
2. **Dashboard** - See all 18 students with risk levels
3. **Click a student** - View detailed analysis
4. **See graphs** - Visual trends over 4 quarters
5. **Edit teacher comment** - Add observations and watch AI re-analyze!

---

## 🎯 Recommended Demo Flow

1. **Start with עידו לוי (Yellow):**
   - Grade 82 (good!) but absences rising
   - Shows early detection in action

2. **Try editing teacher comment:**
   - Scroll to teacher section
   - Add: "נראה לחוץ - בעיות בבית"
   - Click "שמור הערה"
   - Watch AI update the analysis!

3. **Check high-risk student יונתן דוד (Red):**
   - See multiple risk factors
   - Urgent recommendations

---

## 📞 Still Having Issues?

1. Make sure no other application is using port 5000
2. Try restarting Visual Studio
3. Run as Administrator if needed
4. Check Windows Firewall isn't blocking localhost

---

## ✅ System is Ready!

Your application is now configured to run smoothly without port conflicts.

**Just double-click START.bat or run from Visual Studio!**

🎉 **Happy demoing!**
