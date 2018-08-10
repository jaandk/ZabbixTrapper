FROM microsoft/dotnet:2.1-sdk as builder
#WORKDIR /sln
#COPY ./ZabbixTrapper.sln ./

#COPY ./ZabbixTrapper/ZabbixTrapper.csproj ./ZabbixTrapper/ZabbixTrapper.csproj
#RUN dotnet restore

#COPY ./ZabbixTrapper /ZabbixTrapper  
COPY . ./
#RUN dotnet build -c Release --no-restore

RUN dotnet publish "./ZabbixTrapper/ZabbixTrapper.csproj" -c Release -o "./out"

FROM microsoft/dotnet:2.1-runtime
#WORKDIR /app  
ENV ASPNETCORE_ENVIRONMENT Local  
ENTRYPOINT ["dotnet", "/ZabbixTrapper/out/zabbixtrapper.dll"]
COPY --from=builder /ZabbixTrapper/out .
