FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["BlazorWASM/BlazorWASM.csproj", "BlazorWASM/"]
RUN dotnet restore "BlazorWASM/BlazorWASM.csproj"
COPY . .
WORKDIR "/src/BlazorWASM"
RUN dotnet build "BlazorWASM.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BlazorWASM.csproj" -c Release -o /app/publish

FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html
COPY --from=publish /app/publish/wwwroot .
COPY BlazorWASM/nginx.conf /etc/nginx/nginx.conf
EXPOSE 80 