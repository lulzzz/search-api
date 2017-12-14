# requires postgres_connection environment variable for startup

FROM microsoft/dotnet:sdk AS build-env

ENV http_proxy='http://10.3.80.80:3128'
ENV https_proxy='http://10.3.80.80:3128'

COPY . ~/api

WORKDIR ~/api

RUN dotnet restore ./Mongo/Mongo.Bootstrapper/Mongo.Bootstrapper.csproj && dotnet publish ./Mongo/Mongo.Bootstrapper/Mongo.Bootstrapper.csproj -c Release -o /out


FROM microsoft/aspnetcore:2.0.3

WORKDIR /app

COPY --from=build-env /out ./

CMD ["dotnet", "Mongo.Bootstrapper.dll"]

