FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/Services/Orders/Orders.API/Orders.API.csproj", "src/Services/Orders/Orders.API/"]
COPY ["src/Services/Orders/Orders.Application/Orders.Application.csproj", "src/Services/Orders/Orders.Application/"]
COPY ["src/Services/Orders/Orders.Domain/Orders.Domain.csproj", "src/Services/Orders/Orders.Domain/"]
COPY ["src/Services/Orders/Orders.Infrastructure/Orders.Infrastructure.csproj", "src/Services/Orders/Orders.Infrastructure/"]
COPY ["src/Shared/", "src/Shared/"]

RUN dotnet restore "src/Services/Orders/Orders.API/Orders.API.csproj"
COPY . .
RUN dotnet build "src/Services/Orders/Orders.API/Orders.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/Services/Orders/Orders.API/Orders.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Orders.API.dll"]
