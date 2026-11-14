#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Build.props", "./"]
COPY ["FpvBattle.Web/FpvBattle.Web.csproj", "FpvBattle.Web/"]
COPY ["FpvBattle.Infrastructure/FpvBattle.Infrastructure.csproj", "FpvBattle.Infrastructure/"]
COPY ["FpvBattle.Domain/FpvBattle.Domain.csproj", "FpvBattle.Domain/"]
COPY ["FpvBattle.Logic/FpvBattle.Logic.csproj", "FpvBattle.Logic/"]
COPY ["FpvBattle.Hangfire.Metrics/FpvBattle.Hangfire.Metrics.csproj", "FpvBattle.Hangfire.Metrics/"]
COPY ["FpvBattle.Mcp/FpvBattle.Mcp.csproj", "FpvBattle.Mcp/"]
RUN dotnet restore "./FpvBattle.Web/FpvBattle.Web.csproj"
COPY . .
RUN rm -f Directory.Build.user.props
WORKDIR "/src/FpvBattle.Web"
RUN dotnet build "./FpvBattle.Web.csproj" -c $BUILD_CONFIGURATION /p:OpenApiGenerateDocuments=false -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./FpvBattle.Web.csproj" -c $BUILD_CONFIGURATION /p:OpenApiGenerateDocuments=false -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FpvBattle.Web.dll"]
