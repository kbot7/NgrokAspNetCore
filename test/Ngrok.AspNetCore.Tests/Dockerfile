#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["Ngrok.AspNetCore.Tests/Ngrok.AspNetCore.Tests.csproj", "Ngrok.AspNetCore.Tests/"]
COPY ["Ngrok.AspNetCore/Ngrok.AspNetCore.csproj", "Ngrok.AspNetCore/"]
RUN ls
RUN dotnet restore "Ngrok.AspNetCore.Tests/Ngrok.AspNetCore.Tests.csproj"
COPY . .
WORKDIR "/src/Ngrok.AspNetCore.Tests"
ENTRYPOINT ["dotnet", "test"]
#CMD tail -f /dev/null