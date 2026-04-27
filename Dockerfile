FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY SeverstalWarehouse.slnx ./
COPY SeverstalWarehouse.Api/SeverstalWarehouse.Api.csproj SeverstalWarehouse.Api/
COPY SeverstalWarehouse.Tests/SeverstalWarehouse.Tests.csproj SeverstalWarehouse.Tests/
RUN dotnet restore SeverstalWarehouse.slnx

COPY . .
RUN dotnet publish SeverstalWarehouse.Api/SeverstalWarehouse.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "SeverstalWarehouse.Api.dll"]

