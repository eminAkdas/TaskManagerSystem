# Çalışma zamanı (Runtime) için en hafif .NET 9 İmajını kullanıyoruz
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
# Yeni .NET versiyonlarında varsayılan port 8080'dir.
EXPOSE 8080

# Derleme (Build) işlemi için ağır olan SDK imajını kullanıyoruz
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Proje (csproj) dosyalarını kopyala ve restore (paket yükleme) işlemini yap
COPY ["TaskManagerSystem.API/TaskManagerSystem.API.csproj", "TaskManagerSystem.API/"]
COPY ["TaskManagerSystem.Business/TaskManagerSystem.Business.csproj", "TaskManagerSystem.Business/"]
COPY ["TaskManagerSystem.Core/TaskManagerSystem.Core.csproj", "TaskManagerSystem.Core/"]
COPY ["TaskManagerSystem.Data/TaskManagerSystem.Data.csproj", "TaskManagerSystem.Data/"]
RUN dotnet restore "TaskManagerSystem.API/TaskManagerSystem.API.csproj"

# Tüm kodları asıl klasöre kopyala ve Build al
COPY . .
WORKDIR "/src/TaskManagerSystem.API"
RUN dotnet build "TaskManagerSystem.API.csproj" -c Release -o /app/build

# Publish al (Sadece gerekli olan DLL dosyalarını /app/publish hedefine çıkart)
FROM build AS publish
RUN dotnet publish "TaskManagerSystem.API.csproj" -c Release -o /app/publish

# Final aşamasında, en üstteki (base) hafif imajı al, Publish edilmiş dosyaları içine at ve başlat.
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TaskManagerSystem.API.dll"]
