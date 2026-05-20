FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY API/PrintFlowApi/PrintFlowApi.csproj API/PrintFlowApi/
RUN dotnet restore API/PrintFlowApi/PrintFlowApi.csproj

COPY API/PrintFlowApi/ API/PrintFlowApi/
RUN dotnet publish API/PrintFlowApi/PrintFlowApi.csproj \
    --configuration Release \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

CMD ["sh", "-c", "ASPNETCORE_URLS=http://0.0.0.0:${PORT:-8080} dotnet PrintFlowApi.dll"]
