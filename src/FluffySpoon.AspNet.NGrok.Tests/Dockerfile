#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["FluffySpoon.AspNet.NGrok.Tests/FluffySpoon.AspNet.NGrok.Tests.csproj", "FluffySpoon.AspNet.NGrok.Tests/"]
COPY ["FluffySpoon.AspNet.NGrok/FluffySpoon.AspNet.NGrok.csproj", "FluffySpoon.AspNet.NGrok/"]
RUN ls
RUN dotnet restore "FluffySpoon.AspNet.NGrok.Tests/FluffySpoon.AspNet.NGrok.Tests.csproj"
COPY . .
WORKDIR "/src/FluffySpoon.AspNet.NGrok.Tests"
ENTRYPOINT ["dotnet", "test"]
#CMD tail -f /dev/null