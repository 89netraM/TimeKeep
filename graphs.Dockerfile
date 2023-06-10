FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src/rpc
COPY ./rpc ./
WORKDIR /src/graphs
COPY ./graphs/TimeKeep.Graphs.csproj .
RUN dotnet restore
COPY ./graphs .
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT dotnet TimeKeep.Graphs.dll
