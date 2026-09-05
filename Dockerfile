# --- Build stage ---
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

# Copy everything and restore/publish
COPY . .
RUN dotnet publish Path.csproj -c Release -o /app/publish

# --- Runtime stage ---
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
WORKDIR /app

# Copy the published app
COPY --from=build /app/publish .

# students.csv isn't marked as "Content" in the .csproj, so it doesn't
# get copied automatically by `dotnet publish`. Copy it in manually so
# File.ReadAllLines("students.csv") in Program.cs can find it at runtime.
COPY students.csv ./students.csv

# Render sets $PORT at runtime; Program.cs already reads it via
# Environment.GetEnvironmentVariable("PORT"), so no extra config needed here.
EXPOSE 5000

ENTRYPOINT ["dotnet", "Path.dll"]
