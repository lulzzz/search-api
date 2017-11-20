# requires postgres_connection environment variable for startup

FROM microsoft/dotnet:sdk AS build-env

ENV http_proxy='http://10.3.80.80:3128'
ENV https_proxy='http://10.3.80.80:3128'

COPY . ~/api

WORKDIR ~/api

RUN dotnet restore ./RDKit/Search.RDKit.Api/Search.RDKit.Api.csproj && dotnet publish ./RDKit/Search.RDKit.Api/Search.RDKit.Api.csproj -c Release -o /out


FROM microsoft/aspnetcore:2.0.3

WORKDIR /app

COPY --from=build-env /out ./

ENTRYPOINT ["dotnet", "Search.RDKit.Api.dll"]

