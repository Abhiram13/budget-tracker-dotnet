# Use "--build-arg" to pass this value during docker build;
ARG NUGET_PAT
ARG DOTNET_VERSION=10.0-alpine

# Pull dotnet image as "base" step
FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS base

# Pass "NUGET_PAT" into "base" image
ARG NUGET_PAT

# Create & Set the directory
WORKDIR /budget-tracker

# Add the private package source using the build argument
RUN dotnet nuget add source --name "github" "https://nuget.pkg.github.com/Abhiram13/index.json" --username Abhiram13 --password "${NUGET_PAT}" --store-password-in-clear-text

# Create new image "restore" from "base" image that already contains github nuget source
FROM base AS restore

# Working directory is set to "/bugdet-tracker"
# Copy current directory files into docker's working directory and restore
COPY budget-tracker.sln ./
COPY Directory.Build.props ./
COPY Directory.Packages.props ./
COPY /Src/BudgetTracker.Finance/ ./BudgetTracker.Finance/
RUN dotnet restore BudgetTracker.Finance/BudgetTracker.Finance.csproj

# Create new image "dev" from "restore" that contains all the build files to run the app. 
FROM restore AS dev

# Set the workdir to app location.
WORKDIR /budget-tracker/BudgetTracker.Finance

# Command to run the app
CMD ["dotnet", "watch", "run"]