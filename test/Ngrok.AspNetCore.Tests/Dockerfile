#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["test/Ngrok.AspNetCore.Tests/Ngrok.AspNetCore.Tests.csproj", "test/Ngrok.AspNetCore.Tests/"]
COPY ["sample/Ngrok.AspNetCore.Sample/Ngrok.AspNetCore.Sample.csproj", "sample/Ngrok.AspNetCore.Sample/"]
RUN dotnet restore "test/Ngrok.AspNetCore.Tests/Ngrok.AspNetCore.Tests.csproj"
COPY . .
WORKDIR "/src/test/Ngrok.AspNetCore.Tests"
ENTRYPOINT ["dotnet", "test"]