FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/Services/Payments/Payments.API/Payments.API.csproj", "src/Services/Payments/Payments.API/"]
COPY ["src/Services/Payments/Payments.Application/Payments.Application.csproj", "src/Services/Payments/Payments.Application/"]
COPY ["src/Services/Payments/Payments.Domain/Payments.Domain.csproj", "src/Services/Payments/Payments.Domain/"]
COPY ["src/Services/Payments/Payments.Infrastructure/Payments.Infrastructure.csproj", "src/Services/Payments/Payments.Infrastructure/"]
COPY ["src/Shared/", "src/Shared/"]

RUN dotnet restore "src/Services/Payments/Payments.API/Payments.API.csproj"
COPY . .
RUN dotnet build "src/Services/Payments/Payments.API/Payments.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/Services/Payments/Payments.API/Payments.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Payments.API.dll"]
