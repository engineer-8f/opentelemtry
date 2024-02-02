FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /sdk
 
ARG BUILD_VERSION=0.0.1
ARG PROJECT_PATH="Osint.Sources/Osint.Source.Two/Osint.Source.Two.csproj"
 
COPY . .
RUN dotnet restore $PROJECT_PATH
RUN dotnet build $PROJECT_PATH -c Release -o /app/build
 
FROM build AS publish
RUN dotnet publish $PROJECT_PATH -c Release -p:Version=${BUILD_VERSION} -o /app/publish
 
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime
WORKDIR /app
 
FROM runtime AS final   
WORKDIR /app
COPY --from=publish /app/publish .
 
# Install OpenTelemetry .NET Automatic Instrumentation
ARG OTEL_VERSION=1.1.0
ENV OTEL_DOTNET_AUTO_HOME="/otel-dotnet-auto"
ADD https://github.com/open-telemetry/opentelemetry-dotnet-instrumentation/releases/download/v${OTEL_VERSION}/otel-dotnet-auto-install.sh otel-dotnet-auto-install.sh
RUN apt-get update  \
    && apt-get install -y unzip curl  \
    && sh otel-dotnet-auto-install.sh

EXPOSE 80

ENTRYPOINT ["dotnet", "Osint.Source.Two.dll"]