FROM mcr.microsoft.com/dotnet/sdk:6.0 AS src
COPY . .

WORKDIR /src
RUN dotnet build MiniBank.Web -c Release -r linux-x64

WORKDIR /src/tests
RUN dotnet test MiniBank.Core.Tests --no-build

WORKDIR /src
RUN dotnet publish MiniBank.Web -c Release -o /app/out -r linux-x64 --self-contained true /p:PublishSingleFile=true

FROM mcr.microsoft.com/dotnet/aspnet:6.0 as final

WORKDIR /app

COPY --from=src /app/out .
ENV ASPNETCORE_URLS=https://localhost:5000;https://localhost:5001
EXPOSE 5000 5001 

ENTRYPOINT ["./MiniBank.Web"]