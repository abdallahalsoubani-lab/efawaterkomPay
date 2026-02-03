FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["src/API/DirectPayGateway.API.csproj", "API/"]
COPY ["src/Core/DirectPayGateway.Core.csproj", "Core/"]
COPY ["src/Infrastructure/DirectPayGateway.Infrastructure.csproj", "Infrastructure/"]

RUN dotnet restore "API/DirectPayGateway.API.csproj"

COPY src/API/ API/
COPY src/Core/ Core/
COPY src/Infrastructure/ Infrastructure/

WORKDIR "/src/API"
RUN dotnet build "DirectPayGateway.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DirectPayGateway.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DirectPayGateway.API.dll"]
