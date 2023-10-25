FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["FileAPI/FileAPI.csproj", "FileAPI/"]
COPY ["Database/Database.csproj", "Database/"]
RUN dotnet restore "FileAPI/FileAPI.csproj"
COPY . .
WORKDIR "/src/FileAPI"
RUN dotnet build "FileAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FileAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FileAPI.dll"]