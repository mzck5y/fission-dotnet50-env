# FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
FROM mcr.microsoft.com/dotnet/runtime-deps:5.0-alpine3.12-amd64 AS base
WORKDIR /app
EXPOSE 8888

# FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine3.12-amd64 AS build
WORKDIR /src
COPY ["fission-dotnet50.csproj", "."]
RUN dotnet restore "./fission-dotnet50.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "fission-dotnet50.csproj" -c Release -o /app/build

FROM build AS publish
# RUN dotnet publish "fission-dotnet50.csproj" -c Release -o /app/publish
RUN dotnet publish "fission-dotnet50.csproj" -c Release -o /app/publish -p:PublishSingleFile=true -r linux-musl-x64 --self-contained true -p:PublishTrimmed=True -p:TrimMode=Link

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["./fission-dotnet50"]

# ENTRYPOINT ["dotnet"]
# CMD ["fission-dotnet50.dll"]

## ENTRYPOINT ["dotnet", "fission-dotnet50.dll"]