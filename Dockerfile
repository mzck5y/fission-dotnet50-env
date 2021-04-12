#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["fission-dotnet50.csproj", "."]
RUN dotnet restore "./fission-dotnet50.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "fission-dotnet50.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "fission-dotnet50.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 8888

ENTRYPOINT ["dotnet"]

CMD ["fission-dotnet50.dll"]

# ENTRYPOINT ["dotnet", "fission-dotnet50.dll"]