FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["casus-oyunu.csproj", "./"]
RUN dotnet restore "casus-oyunu.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "casus-oyunu.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "casus-oyunu.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "casus-oyunu.dll"] 