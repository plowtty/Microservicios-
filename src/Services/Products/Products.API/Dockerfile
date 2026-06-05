FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Services/Products/Products.API/Products.API.csproj", "src/Services/Products/Products.API/"]
COPY ["src/Shared/", "src/Shared/"]
RUN dotnet restore "src/Services/Products/Products.API/Products.API.csproj"
COPY . .
RUN dotnet publish "src/Services/Products/Products.API/Products.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Products.API.dll"]
