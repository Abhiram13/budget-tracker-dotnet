# Use a build argument for the PAT
ARG NUGET_PAT
ARG DOTNET_VERSION=10.0-alpine

FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS base

ARG NUGET_PAT
WORKDIR /budget-tracker

# Add the private package source using the build argument
RUN dotnet nuget add source --name "github" "https://nuget.pkg.github.com/Abhiram13/index.json" --username Abhiram13 --password "${NUGET_PAT}" --store-password-in-clear-text

FROM base AS restore

COPY budget-tracker.sln ./
COPY Directory.Build.props ./
COPY Directory.Packages.props ./
COPY /Src/BudgetTracker.Finance/ ./BudgetTracker.Finance/
RUN dotnet restore BudgetTracker.Finance/BudgetTracker.Finance.csproj

FROM restore AS dev
WORKDIR /budget-tracker/Src/BudgetTracker.Finance
CMD ["dotnet", "watch", "run"]

# Re-declare the build argument to make it available in this stage
#ARG NUGET_PAT

# Copy csproj and restore as distinct layers
# COPY *.csproj ./
# RUN dotnet restore

#RUN dotnet restore BudgetTracker.Api/BudgetTracker.Api.csproj
#RUN dotnet publish BudgetTracker.Api/BudgetTracker.Api.csproj -c Release -o /out

# Copy everything else and build
# COPY . .
# RUN dotnet publish -c Release -o out

# Build runtime image
#FROM mcr.microsoft.com/dotnet/aspnet:8.0
#WORKDIR /app
## COPY --from=build-env /app/out .
#COPY --from=build-env /out ./
#
#ENTRYPOINT [ "dotnet", "/app/BudgetTracker.Api.dll" ]