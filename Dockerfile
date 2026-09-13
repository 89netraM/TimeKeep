FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM node:26 as web-app-build
WORKDIR /src/web-app
COPY ./web-app/package.json ./web-app/package-lock.json ./
RUN npm ci
COPY ./web-app/index.html ./web-app/tsconfig.json ./
COPY ./web-app/src/ ./src/
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src/rpc
COPY ./rpc ./
WORKDIR /src/server
COPY ./server/TimeKeep.csproj .
RUN dotnet restore
COPY ./server .
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=web-app-build /src/web-app/dist ./wwwroot/
ENTRYPOINT dotnet TimeKeep.dll
