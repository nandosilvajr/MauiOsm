FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["src/Itinero.API/Itinero.API.csproj", "src/Itinero.API/"]
RUN dotnet restore "src/Itinero.API/Itinero.API.csproj"
COPY . .
WORKDIR "/src/src/Itinero.API"
RUN dotnet build "Itinero.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Itinero.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY ["src/Itinero.API/data/portugal.routerdb", "/app"]
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Itinero.API.dll"]
    