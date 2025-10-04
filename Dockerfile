# Use a build argument for the PAT
ARG NUGET_PAT
ARG ENV

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

COPY budget-tracker.sln ./

COPY src/BudgetTracker.Api/ ./BudgetTracker.Api/
COPY src/BudgetTracker.Core/ ./BudgetTracker.Core/
COPY src/BudgetTracker.Infrastructure/ ./BudgetTracker.Infrastructure/

# Re-declare the build argument to make it available in this stage
ARG NUGET_PAT
ARG ENV

# Add the private package source using the build argument
RUN dotnet nuget add source --name "github" "https://nuget.pkg.github.com/Abhiram13/index.json" --username Abhiram13 --password "${NUGET_PAT}" --store-password-in-clear-text

# Copy csproj and restore as distinct layers
# COPY *.csproj ./
# RUN dotnet restore

RUN dotnet restore BudgetTracker.Api/BudgetTracker.Api.csproj
RUN dotnet publish BudgetTracker.Api/BudgetTracker.Api.csproj -c Release -o /out

# Copy everything else and build
# COPY . .
# RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
# COPY --from=build-env /app/out .
COPY --from=build-env /out ./

ENTRYPOINT [ "dotnet", "/app/BudgetTracker.Api.dll" ]