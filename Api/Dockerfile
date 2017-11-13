FROM microsoft/dotnet:sdk AS build-env

ENV http_proxy='http://10.3.80.80:3128'
ENV https_proxy='http://10.3.80.80:3128'
ENV no_proxy='search_rdkit-postgres_1'

COPY . ~/api

WORKDIR ~/api

RUN dotnet restore ./Search/Search.csproj

RUN dotnet publish ./Search/Search.csproj -c Release -o /out


FROM microsoft/aspnetcore:2.0

EXPOSE 80

WORKDIR /app

COPY --from=build-env /out ./

ENTRYPOINT ["dotnet", "Search.dll"]

