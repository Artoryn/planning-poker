# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/PlanningPoker.Server/PlanningPoker.Server.csproj", "src/PlanningPoker.Server/"]
COPY ["src/PlanningPoker.Client/PlanningPoker.Client.csproj", "src/PlanningPoker.Client/"]
COPY ["src/PlanningPoker.Hub.Client/PlanningPoker.Hub.Client.csproj", "src/PlanningPoker.Hub.Client/"]
COPY ["src/PlanningPoker.Hub.Client.Abstractions/PlanningPoker.Hub.Client.Abstractions.csproj", "src/PlanningPoker.Hub.Client.Abstractions/"]
COPY ["src/PlanningPoker.Engine.Core/PlanningPoker.Engine.Core.csproj", "src/PlanningPoker.Engine.Core/"]
COPY ["src/PlanningPoker.Engine.Core.Models/PlanningPoker.Engine.Core.Models.csproj", "src/PlanningPoker.Engine.Core.Models/"]
COPY ["src/PlanningPoker.Server.Infrastructure/PlanningPoker.Server.Infrastructure.csproj", "src/PlanningPoker.Server.Infrastructure/"]
RUN dotnet restore "./src/PlanningPoker.Server/PlanningPoker.Server.csproj"
COPY . .
WORKDIR "/src/src/PlanningPoker.Server"
RUN dotnet build "./PlanningPoker.Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./PlanningPoker.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PlanningPoker.Server.dll"]