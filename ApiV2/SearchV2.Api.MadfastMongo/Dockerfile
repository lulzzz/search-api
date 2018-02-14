FROM microsoft/dotnet:sdk AS build-env
COPY . ~/api
WORKDIR ~/api
RUN dotnet restore ./SearchV2.Api.MadfastMongo/SearchV2.Api.MadfastMongo.csproj && dotnet publish ./SearchV2.Api.MadfastMongo/SearchV2.Api.MadfastMongo.csproj -c Release -o /out

FROM microsoft/aspnetcore:2.0.5
WORKDIR /app
COPY --from=build-env /out ./
ENTRYPOINT ["dotnet", "SearchV2.Api.MadfastMongo.dll"]

