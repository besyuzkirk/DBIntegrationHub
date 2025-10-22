# DBIntegrationHub - DevOps & Docker Orchestration

Bu klasör, DBIntegrationHub projesinin Docker orkestrasyon yapılandırmalarını içerir.

## 📋 İçerik

- `docker-compose.yml` - Ana orkestrasyon dosyası
- `Dockerfile.api` - .NET API için Dockerfile
- `Dockerfile.ui` - Next.js UI için Dockerfile
- `.dockerignore` - Docker build'den hariç tutulacak dosyalar
- `.env.example` - Örnek environment değişkenleri
- `init-scripts/` - PostgreSQL başlangıç scriptleri

## 🏗️ Mimari

```
┌─────────────────────────────────────────────────────────┐
│                     Docker Network                      │
│               dbintegrationhub-network                  │
│                                                         │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐ │
│  │              │  │              │  │              │ │
│  │  PostgreSQL  │  │   .NET API   │  │  Next.js UI  │ │
│  │   Port 5432  │◄─┤   Port 5149  │◄─┤   Port 3000  │ │
│  │              │  │              │  │              │ │
│  └──────────────┘  └──────────────┘  └──────────────┘ │
│         ▲                  ▲                  ▲        │
│         │                  │                  │        │
└─────────┼──────────────────┼──────────────────┼────────┘
          │                  │                  │
     Volume: Data        Volume: Logs      Browser Access
```

## 🚀 Hızlı Başlangıç

### Ön Gereksinimler

- Docker Desktop (v20.10+)
- Docker Compose (v2.0+)
- En az 4GB RAM

### Tüm Servisleri Başlatma

```bash
# DevOps klasörüne git
cd devops

# Tüm servisleri başlat (arka planda)
docker-compose up -d

# Logları izle
docker-compose logs -f

# Sadece belirli bir servisin loglarını izle
docker-compose logs -f api
```

### Servislere Erişim

- **Frontend (UI)**: http://localhost:3000
- **Backend API**: http://localhost:5149
- **API Swagger**: http://localhost:5149/swagger
- **PostgreSQL**: localhost:5432
  - Database: `DBIntegrationHub`
  - Username: `postgres`
  - Password: `postgres`

## 🛠️ Komutlar

### Temel Komutlar

```bash
# Servisleri başlat
docker-compose up -d

# Servisleri durdur
docker-compose down

# Servisleri durdur ve volume'leri sil
docker-compose down -v

# Servisleri yeniden başlat
docker-compose restart

# Belirli bir servisi yeniden başlat
docker-compose restart api
```

### Build Komutları

```bash
# Tüm servisleri yeniden build et
docker-compose build

# No-cache ile build et
docker-compose build --no-cache

# Belirli bir servisi build et
docker-compose build api

# Build edip başlat
docker-compose up -d --build
```

### Debug Komutları

```bash
# Çalışan container'ları listele
docker-compose ps

# Tüm servislerin loglarını göster
docker-compose logs

# Son 100 satır log göster
docker-compose logs --tail=100

# Container içine gir
docker-compose exec api bash
docker-compose exec ui sh
docker-compose exec postgres psql -U postgres -d DBIntegrationHub

# Resource kullanımını göster
docker stats
```

### Database Komutları

```bash
# PostgreSQL container'ına bağlan
docker-compose exec postgres psql -U postgres -d DBIntegrationHub

# Database backup al
docker-compose exec postgres pg_dump -U postgres DBIntegrationHub > backup.sql

# Backup'ı geri yükle
docker-compose exec -T postgres psql -U postgres DBIntegrationHub < backup.sql

# Database'i sıfırla (dikkatli kullan!)
docker-compose down -v postgres
docker-compose up -d postgres
```

## 🔧 Konfigürasyon

### Environment Değişkenleri

`.env` dosyası oluşturarak varsayılan değerleri değiştirebilirsiniz:

```bash
cp .env.example .env
# .env dosyasını düzenle
```

### Port Değiştirme

`docker-compose.yml` dosyasında port mapping'leri değiştirebilirsiniz:

```yaml
ports:
  - "3000:3000"  # HOST:CONTAINER
```

### Volume Yönetimi

```bash
# Volume'leri listele
docker volume ls

# Volume detaylarını göster
docker volume inspect devops_postgres_data

# Kullanılmayan volume'leri temizle
docker volume prune
```

## 🔍 Troubleshooting

### Problem: Container başlamıyor

```bash
# Container loglarını kontrol et
docker-compose logs [service-name]

# Container durumunu kontrol et
docker-compose ps
```

### Problem: Database bağlantı hatası

```bash
# PostgreSQL health check durumunu kontrol et
docker-compose ps postgres

# PostgreSQL loglarını kontrol et
docker-compose logs postgres

# Database'e manuel bağlan
docker-compose exec postgres psql -U postgres
```

### Problem: Port zaten kullanımda

```bash
# Portu kullanan işlemi bul (macOS/Linux)
lsof -i :5432
lsof -i :5149
lsof -i :3000

# İşlemi sonlandır veya docker-compose.yml'de portu değiştir
```

### Problem: Build hatası

```bash
# Cache'i temizle ve yeniden build et
docker-compose build --no-cache

# Docker sistem temizliği
docker system prune -a
```

## 📊 Monitoring & Logs

### Log Yönetimi

```bash
# Tüm loglar
docker-compose logs

# Specific service
docker-compose logs api

# Follow mode
docker-compose logs -f

# Timestamp ile
docker-compose logs -f --timestamps

# Son N satır
docker-compose logs --tail=50 api
```

### Health Checks

```bash
# API health check
curl http://localhost:5149/health

# PostgreSQL health check
docker-compose exec postgres pg_isready -U postgres
```

## 🔐 Production Notları

Production ortamında kullanmadan önce:

1. ✅ **Güvenlik**: Varsayılan şifreleri değiştirin
2. ✅ **HTTPS**: Reverse proxy (nginx/traefik) ekleyin
3. ✅ **Backup**: Otomatik backup stratejisi oluşturun
4. ✅ **Monitoring**: Prometheus/Grafana gibi monitoring ekleyin
5. ✅ **Secrets**: Hassas bilgileri Docker secrets veya vault kullanın
6. ✅ **Resources**: CPU/Memory limitleri ekleyin
7. ✅ **Networks**: Production network izolasyonu yapın

### Resource Limits Örneği

```yaml
services:
  api:
    deploy:
      resources:
        limits:
          cpus: '2'
          memory: 2G
        reservations:
          cpus: '1'
          memory: 1G
```

## 🧹 Temizlik

```bash
# Tüm container'ları durdur ve sil
docker-compose down

# Volume'ler dahil tümünü sil
docker-compose down -v

# Tüm Docker sistemini temizle (DİKKAT!)
docker system prune -a --volumes
```

## 📚 Kaynaklar

- [Docker Documentation](https://docs.docker.com/)
- [Docker Compose Documentation](https://docs.docker.com/compose/)
- [.NET Docker Images](https://hub.docker.com/_/microsoft-dotnet)
- [Node.js Docker Images](https://hub.docker.com/_/node)
- [PostgreSQL Docker Images](https://hub.docker.com/_/postgres)

## 🆘 Yardım

Sorun yaşarsanız:

1. Container loglarını kontrol edin
2. Health check durumlarını kontrol edin
3. Network bağlantısını kontrol edin
4. Issue açın veya dokümantasyonu kontrol edin

---

**Created with ❤️ for DBIntegrationHub**

