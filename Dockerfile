FROM microsoft/dotnet:2.1-sdk-alpine AS build
WORKDIR /app

COPY *.sln .
COPY WeeklyReddit/*.csproj ./WeeklyReddit/
RUN dotnet restore

COPY WeeklyReddit/. ./WeeklyReddit/
WORKDIR /app/WeeklyReddit
RUN dotnet publish -c Release -o out

FROM microsoft/dotnet:2.1-aspnetcore-runtime-alpine AS runtime
WORKDIR /app
COPY --from=build /app/WeeklyReddit/out ./
ENTRYPOINT ["dotnet", "WeeklyReddit.dll"]
