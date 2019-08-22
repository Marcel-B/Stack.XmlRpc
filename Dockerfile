FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
#FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
EXPOSE 5042

FROM microsoft/dotnet:2.2-sdk AS build

#FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY XmlRpc.csproj XmlRpc/
RUN dotnet restore "XmlRpc/XmlRpc.csproj"
COPY . "XmlRpc"
WORKDIR "/src/XmlRpc"
RUN dotnet build "XmlRpc.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "XmlRpc.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "XmlRpc.dll"]
