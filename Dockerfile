FROM microsoft/dotnet:2.0-sdk AS build-env
WORKDIR /app

# copy csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet restore

# copy and build everything else
COPY . ./
RUN dotnet publish -c Release -o out

# build runtime image
FROM microsoft/dotnet:2.0-runtime
WORKDIR /app
COPY --from=build-env /app/out ./

# setup environment variables
ENV "reddit__username"="replace-me" \
  "reddit__password"="replace-me" \
  "reddit__clientId"="replace-me" \
  "reddit__clientSecret"="replace-me" \
  "smtpSettings__server"="replace-me" \
  "smtpSettings__port"="replace-me" \
  "smtpSettings__username"="replace-me" \
  "smtpSettings__password"="replace-me" \
  "emailSettings__fromAddress"="replace-me" \
  "emailSettings__toAddress"="replace-me"

ENTRYPOINT ["dotnet", "WeeklyReddit.dll"]
