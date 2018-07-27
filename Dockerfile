FROM microsoft/dotnet:2.1-sdk AS builder  
WORKDIR /sln
COPY ./ZabbixTrapper.sln ./

COPY ./ZabbixTrapper/ZabbixTrapper.csproj /.ZabbixTrapper/ZabbixTrapper.csproj
RUN dotnet restore

COPY ./ZabbixTrapper ./ZabbixTrapper  
RUN dotnet build -c Release --no-restore

RUN dotnet publish "./ZabbixTrapper/ZabbixTrapper.csproj" -c Release -o "../../dist" --no-restore

FROM microsoft/dotnet:2.1-runtime  
WORKDIR /app  
ENV ASPNETCORE_ENVIRONMENT Local  
ENTRYPOINT ["dotnet", "out/ZabbixTrapper.dll"]
COPY --from=builder /sln/dist .
