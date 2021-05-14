FROM mcr.microsoft.com/dotnet/runtime:5.0
COPY bin/Release/net5/publish/ ./

ENTRYPOINT ["dotnet", "WeeklyReddit.dll"]